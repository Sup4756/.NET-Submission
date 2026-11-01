"""HTTP integration tests against running applications (real database, no mocks).
Install: python -m pip install requests beautifulsoup4
Run: python tests/smoke.py [http://127.0.0.1:5101 ...]
Use disposable development databases: these checks create accounts and orders.
"""
import sys, uuid, html
from urllib.parse import urljoin
import requests
from bs4 import BeautifulSoup

def run(base):
    user=requests.Session(); admin=requests.Session(); outsider=requests.Session()
    count=0
    def check(condition,label):
        nonlocal count
        assert condition, f'{base}: {label}'
        count+=1
    def get(session,path):
        r=session.get(base+path,timeout=30)
        check(r.status_code==200,f'GET {path}: {r.status_code}')
        return r
    def post(session,path,field,values,formname=None):
        r=get(session,path); soup=BeautifulSoup(html.unescape(r.text),'html.parser')
        forms=soup.select('form')
        form=next((f for f in forms if f.select_one(f'[name="{field}"]') and (not formname or f.select_one(f'[name="_handler"][value="{formname}"]'))),None)
        assert form is not None, (path,field,[str(f)[:180] for f in forms])
        data={i['name']:i.get('value','') for i in form.select('input[name]') if i.get('type') not in ['submit','button','checkbox']}
        for i in form.select('textarea[name]'):data[i['name']]=i.text
        for i in form.select('select[name]'):
            option=i.select_one('option[selected]') or i.select_one('option')
            data[i['name']]=option.get('value',option.text) if option else ''
        data.update(values)
        result=session.post(urljoin(r.url,form.get('action') or r.url),data=data,timeout=30)
        check(result.status_code==200,f'POST {path}: {result.status_code}: {result.text[:300]}')
        return result
    def delete_row(session,path):
        r=get(session,path)
        form=BeautifulSoup(html.unescape(r.text),'html.parser').select_one('td.actions form')
        assert form is not None, path
        data={i['name']:i.get('value','') for i in form.select('input[name]')}
        r=session.post(urljoin(r.url,form.get('action') or r.url),data=data,timeout=30)
        check(r.status_code==200,'delete row accepted')
        return r
    email=f'smoke-{uuid.uuid4().hex[:8]}@example.test'
    get(user,'/');r=get(user,'/Products');check('Air Motion' in html.unescape(r.text),'catalog database seed')
    r=get(user,'/Products?q=does-not-exist');check('Không tìm thấy sản phẩm' in html.unescape(r.text),'catalog search')
    r=get(user,'/Products?page=2');check('Cloud Runner' in html.unescape(r.text),'pagination')
    check(user.get(base+'/Products/999999').status_code==404,'missing product 404')
    r=post(user,'/Account/Register','Input.Email',{'Input.FullName':'Smoke Customer','Input.Email':email,'Input.Password':'Testing@12345','Input.ConfirmPassword':'Testing@12345'})
    check(r.url.endswith('/Products'),'registration assigns User and redirects')
    denied=user.get(base+'/Admin',allow_redirects=False);check(denied.status_code in [302,403],'User cannot open admin')
    r=post(user,'/Products/1','quantity',{'quantity':'2'});check('Đã thêm' in html.unescape(r.text),'add to database cart')
    r=get(user,'/Cart');check('Air Motion' in html.unescape(r.text),'cart persisted across requests')
    post(outsider,'/Account/Login','Input.Email',{'Input.Email':'user@stepshoes.local','Input.Password':'Demo@12345'})
    r=get(outsider,'/Cart');check('Giỏ hàng đang trống' in html.unescape(r.text),'carts isolated per account')
    r=post(user,'/Cart','Input.Recipient',{'Input.Recipient':'Smoke Customer','Input.Phone':'0901234567','Input.ShippingAddress':'12 Test Street, Hồ Chí Minh'})
    check(r.url.endswith('/Orders') and 'Air Motion' in html.unescape(r.text),'checkout creates order')
    r=get(user,'/Cart');check('Giỏ hàng đang trống' in html.unescape(r.text),'checkout clears database cart')
    r=get(outsider,'/Orders');check('Air Motion' not in html.unescape(r.text),'orders isolated per account')
    r=post(user,'/Profile','Input.FullName',{'Input.FullName':'Smoke Updated','Input.Email':email,'Input.City':'Hà Nội','Input.PhoneNumber':'0901234567','Input.Gender':'Khác','Input.Address':'Test Address','Input.DateOfBirth':'2000-01-02'})
    check('Đã lưu' in html.unescape(r.text),'user can edit own profile')
    r=post(admin,'/Account/Login','Input.Email',{'Input.Email':'admin@stepshoes.local','Input.Password':'Demo@12345'})
    check(r.url.endswith('/Admin'),'Admin login redirects to dashboard')
    # Cancel the order created by this run, exercising admin status handling and returning stock.
    r=post(admin,'/Admin/Orders','status',{'status':'Đã hủy'})
    check('Đã hủy' in html.unescape(r.text),'admin can cancel a new order')
    for path in ['/Admin/Products','/Admin/Orders','/Admin/Accounts','/Admin/Customers']:
        get(admin,path)
    title='Smoke Shoe '+uuid.uuid4().hex[:5]
    r=post(admin,'/Admin/Products/Edit','Input.Name',{'Input.Name':title,'Input.Brand':'Test','Input.Category':'Test','Input.Price':'1250000','Input.Stock':'7','Input.Description':'Integration test shoe'})
    check(r.url.endswith('/Admin/Products') and title in html.unescape(r.text),'admin creates product')
    r=get(admin,'/Admin/Products?q='+title);soup=BeautifulSoup(html.unescape(r.text),'html.parser')
    edit=next(a['href'] for a in soup.select('a[href]') if '/Admin/Products/Edit/' in a['href'])
    pid=edit.rsplit('/',1)[-1]
    r=post(admin,edit,'Input.Name',{'Input.Name':title+' Updated','Input.Brand':'Test','Input.Category':'Test','Input.Price':'1350000','Input.Stock':'8','Input.Description':'Updated test shoe'})
    check(title+' Updated' in html.unescape(r.text),'admin updates product')
    # CSRF protection must reject state-changing requests without tokens.
    r=get(admin,'/Admin/Products?q='+title)
    delete_form=BeautifulSoup(r.text,'html.parser').select_one('td.actions form')
    target=urljoin(r.url,delete_form.get('action') or r.url)
    r=admin.post(target,data={'id':pid},allow_redirects=False)
    check(r.status_code==400,'CSRF rejected')
    # Account filtering, role update, locking and soft deletion.
    r=get(admin,'/Admin/Accounts?q='+email);soup=BeautifulSoup(html.unescape(r.text),'html.parser')
    edit=next(a['href'] for a in soup.select('a[href]') if '/Admin/Accounts/Edit/' in a['href'])
    uid=edit.rsplit('/',1)[-1]
    values={'Input.FullName':'Smoke Updated','Input.Email':email,'Input.City':'Hà Nội','Input.Role':'Admin','Input.Locked':'false'}
    post(admin,edit,'Input.FullName',values)
    r=user.get(base+'/Admin',allow_redirects=False);check(r.status_code==302,'role change invalidates existing session')
    r=post(user,'/Account/Login','Input.Email',{'Input.Email':email,'Input.Password':'Testing@12345'});check(r.url.endswith('/Admin'),'changed role applied at login')
    values['Input.Locked']='true'
    post(admin,edit,'Input.FullName',values)
    r=user.get(base+'/Admin',allow_redirects=False);check(r.status_code==302,'locked account session invalidated')
    r=post(user,'/Account/Login','Input.Email',{'Input.Email':email,'Input.Password':'Testing@12345'});check('Đăng nhập không thành công' in html.unescape(r.text),'locked account cannot sign in')
    r=get(admin,'/Admin/Accounts?locked=true');check(email in html.unescape(r.text),'locked account filter')
    delete_row(admin,'/Admin/Accounts?q='+email)
    r=get(admin,'/Admin/Accounts?q='+email);check(email not in BeautifulSoup(r.text,'html.parser').get_text(),'soft-deleted account hidden')
    r=post(user,'/Account/Login','Input.Email',{'Input.Email':email,'Input.Password':'Testing@12345'});check('Đăng nhập không thành công' in html.unescape(r.text),'soft-deleted account cannot log in')
    r=post(user,'/Account/Register','Input.Email',{'Input.FullName':'Reused Account','Input.Email':email,'Input.Password':'Testing@12345','Input.ConfirmPassword':'Testing@12345'})
    check(r.url.endswith('/Account/Register') and 'Email này đã được sử dụng' in html.unescape(r.text),'deleted account email remains reserved without server error')
    delete_row(admin,'/Admin/Products?q='+title)
    r=get(admin,'/Products?q='+title);check(title not in BeautifulSoup(html.unescape(r.text),'html.parser').get_text(),'deleted product removed from catalog')
    check(admin.get(base+'/Products/'+pid).status_code==404,'deleted product details unavailable')
    r=admin.post(base+'/Account/Logout',data={},allow_redirects=False)
    check(r.status_code==400,'logout requires CSRF token')
    r=get(admin,'/Admin')
    form=BeautifulSoup(r.text,'html.parser').select_one('form[action="/Account/Logout"]')
    data={i['name']:i.get('value','') for i in form.select('input[name]')}
    r=admin.post(base+'/Account/Logout',data=data)
    check(r.status_code==200 and r.url.endswith('/'),'logout returns home')
    check(admin.get(base+'/Admin',allow_redirects=False).status_code==302,'logged-out session cannot access admin')
    print(f'{base}: {count} checks passed',flush=True)
    return count

if __name__=='__main__':
    total=sum(run(base.rstrip('/')) for base in (sys.argv[1:] or ['http://127.0.0.1:5101','http://127.0.0.1:5102','http://127.0.0.1:5103']))
    print(f'Total: {total} checks passed')
