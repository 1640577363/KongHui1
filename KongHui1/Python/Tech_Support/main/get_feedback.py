# 加载配置
import json

from config.read_config import load_config
from request.request import post_request,get_request
from login import load_token_from_config, get_new_token

config = load_config()
ip = config['remoteHost']['ip']
port = config['remoteHost']['port']
url = f'http://{ip}:{port}/Issues_support/Issues_support/list'

def send_get_feedback_list(url, token, params):
    # data=json.dumps(data)
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

def get_feedback_list(hardwareId=None):
    token=load_token_from_config()
    params={
        'hardwareId':hardwareId,
        'status':1,
    }
    # result1=requests.post(url,token,data=json.dumps(data))
    result1=send_get_feedback_list(url, token=token, params=params)
    rows1=result1['rows']
    # 如果响应状态码不为 200，刷新 token 并重新请求
    if not result1 or result1.get('code') != 200:
        print("token 失效，刷新 token...")
        token = get_new_token()  # 获取新 token
        result1 = send_get_feedback_list(url, token=token, params=params)
        print(result1)

    # 再次查询状态 2 的反馈列表
    params = {
        'hardwareId':hardwareId,
        'status': 2,}
    result2 = send_get_feedback_list(url, token=token, params=params)
    rows2=result2['rows']
    # print(result2)
    combined_rows = rows1 + rows2

    # 存储为 JSON 文件
    with open('FeedbackQuestion.json', 'w', encoding='utf-8') as f:
        json.dump(combined_rows, f, ensure_ascii=False, indent=4)

    print(f"成功将数据存储到 feedback_combined.json 文件中。")
    print(combined_rows)  # 打印合并后的结果
# get_feedback_list('47F11276-B486-4A35-BA83-95E70FF561B6')
get_feedback_list()