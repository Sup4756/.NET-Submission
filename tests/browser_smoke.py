"""Browser checks; pip install playwright && playwright install chromium.
CHROMIUM_PATH can point at an installed Chromium instead of the Playwright download.
Run against running development apps; screenshots go to ignored artifacts/screenshots.
"""
import os
from pathlib import Path
from playwright.sync_api import sync_playwright

output=Path(__file__).resolve().parents[1]/'artifacts'/'screenshots'
output.mkdir(parents=True,exist_ok=True)
checks=0
with sync_playwright() as p:
    options={'headless':True}
    if os.environ.get('CHROMIUM_PATH'):options['executable_path']=os.environ['CHROMIUM_PATH']
    browser=p.chromium.launch(**options)
    for port in [5101,5102,5103]:
        context=browser.new_context(viewport={'width':1440,'height':1000})
        page=context.new_page()
        errors=[]
        page.on('pageerror',lambda e: errors.append(str(e)))
        for width in [1440,390]:
            page.set_viewport_size({'width':width,'height':900})
            for path in ['/','/Products','/Products/1','/Account/Login']:
                response=page.goto(f'http://127.0.0.1:{port}{path}',wait_until='networkidle')
                assert response.status==200,(port,path,response.status)
                assert not page.evaluate('document.documentElement.scrollWidth > innerWidth'),(port,width,path,'overflow')
                checks+=2
            page.goto(f'http://127.0.0.1:{port}/Products',wait_until='networkidle')
            page.locator('input[name="q"]').last.fill('Air Motion')
            page.get_by_role('button',name='Lọc sản phẩm',exact=True).click()
            page.wait_for_load_state('networkidle')
            assert page.locator('.product-card').count()==1,(port,'filter')
            checks+=1
        page.set_viewport_size({'width':1440,'height':1000})
        page.goto(f'http://127.0.0.1:{port}/Account/Login')
        page.locator('[name="Input.Email"]').fill('admin@stepshoes.local')
        page.locator('[name="Input.Password"]').fill('Demo@12345')
        page.get_by_role('button',name='Đăng nhập →',exact=True).click()
        page.wait_for_url(f'http://127.0.0.1:{port}/Admin')
        assert page.locator('.sidebar').is_visible()
        checks+=1
        page.screenshot(path=str(output/f'{port}-admin.png'),full_page=True)
        page.goto(f'http://127.0.0.1:{port}/Admin/Accounts',wait_until='networkidle')
        assert 'Admin' in page.locator('table').inner_text(),(port,'account roles displayed')
        checks+=1
        page.set_viewport_size({'width':390,'height':900})
        assert not page.evaluate('document.documentElement.scrollWidth > innerWidth'),(port,'admin overflow')
        checks+=1
        page.goto(f'http://127.0.0.1:{port}/',wait_until='networkidle')
        page.screenshot(path=str(output/f'{port}-mobile.png'),full_page=True)
        page.set_viewport_size({'width':1440,'height':1000})
        page.screenshot(path=str(output/f'{port}-home.png'),full_page=True)
        assert not errors,(port,errors)
        checks+=1
        context.close()
    browser.close()
print(f'Browser: {checks} checks passed')
