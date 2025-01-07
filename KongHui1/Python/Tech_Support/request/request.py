import requests
import json

def post_request(url,headers,data):
    # 设置请求头

    data=json.dumps(data)
    try:
        # 发送 POST 请求
        response = requests.post(url, data=data, headers=headers)
        # 返回响应对象，以便后续处理（可选）
        return response
    except requests.exceptions.RequestException as e:
        # 捕获网络请求异常并输出错误信息
        print(f"请求错误: {e}")
        return None

def get_request(url,token,params):
    # 设置请求头
    headers = {
        'Authorization': f"Bearer {token}",  # 在 Authorization 头部使用 "Bearer" 关键字，通常更常见
        'Content-Type': 'application/json'
    }
    try:
        # 发送 get 请求
        response = requests.get(url, headers=headers,params=params)
        # 返回响应对象，以便后续处理（可选）
        return response
    except requests.exceptions.RequestException as e:
        # 捕获网络请求异常并输出错误信息
        print(f"请求错误: {e}")
        return None