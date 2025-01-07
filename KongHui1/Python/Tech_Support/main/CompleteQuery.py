#整机查询测试
# 加载配置
import json


from config.read_config import load_config
from request.request import post_request,get_request
from login import load_token_from_config, get_new_token

config = load_config()
ip = config['remoteHost']['ip']
port = config['remoteHost']['port']
url = f'http://{ip}:{port}/warranty/warranty/list'

def send_get_pcs_list(url, token, params):
    headers = {
        'Authorization': f"Bearer {token}",  # 在 Authorization 头部使用 "Bearer" 关键字，通常更常见
        'Content-Type': 'application/json'
    }
    res=get_request(url,token,params=params)
    if  res.status_code == 200:
        # 假设响应内容是 JSON 格式，可以将其解析为字典
        try:
            response_data = res.json()  # 如果 response 是一个 JSON 响应，直接解析
            return response_data
        except json.JSONDecodeError:
            print("无法解析响应的 JSON 数据")
            return None
    else:
        print(f"请求失败，状态码: {res.status_code if res else '无响应'}")
        return None

def get_feedback_list(hardware_id=None):
    token=load_token_from_config()
    params={
        'addr':hardware_id,
    }
    result=send_get_pcs_list(url, token=token, params=params)
    # 如果响应状态码不为 200，刷新 token 并重新请求
    if not result or result.get('code') != 200:
        print("token 失效，刷新 token...")
        token = get_new_token()  # 获取新 token
        result = send_get_pcs_list(url, token=token, params=params)
        print(result)

    rows=result['rows']
    # 存储为 JSON 文件
    with open('Warranty.json', 'w', encoding='utf-8') as f:
        json.dump(rows, f, ensure_ascii=False, indent=4)

    print(f"成功将数据存储到 feedback_combined.json 文件中。")
    print(rows)  # 打印合并后的结果
get_feedback_list('67G11299-H406-4E35-BM83-95E70FR561T0')
# get_feedback_list()