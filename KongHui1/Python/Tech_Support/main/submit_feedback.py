# 加载配置
import json

from config.read_config import load_config
from request.request import post_request, get_request
from login import load_token_from_config, get_new_token

config = load_config()
ip = config['remoteHost']['ip']
port = config['remoteHost']['port']
url = f'http://{ip}:{port}/Issues_support/Issues_support'


def send_submit_feedback(url, token, data):
    # data=json.dumps(data)
    headers = {
        'Authorization': f"Bearer {token}",  # 在 Authorization 头部使用 "Bearer" 关键字，通常更常见
        'Content-Type': 'application/json'
    }
    res = post_request(url, headers, data)
    if res.status_code == 200:
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


class Feedback:
    hardwareId=''
    problemType = 0
    problemDescription = ''
    contactName= ''
    contactPhone = ''
    company = ''
    hostType = ''

    def __init__(self, hardware_id, problem_type, problem_description,contact_phone, company, host_type, contact_name=None):
        self.hardwareId = hardware_id
        self.problemType = problem_type
        self.problemDescription = problem_description
        self.contactName=contact_name
        self.contactPhone = contact_phone
        self.company = company
        self.hostType = host_type


def submit_feedback(fd: Feedback):
    token = load_token_from_config()

    data = {
        'issueId':None,
        'hardwareId': fd.hardwareId,
        'problemType':fd.problemType,
        'problemDescription': fd.problemDescription,
        'contactPhone': fd.contactPhone,
        'company': fd.company,
        'hostType': fd.hostType
    }
    result = send_submit_feedback(url, token=token, data=data)
    # 如果响应状态码不为 200，刷新 token 并重新请求
    if not result or result.get('code') != 200:
        print("token 失效，刷新 token...")
        token = get_new_token()  # 获取新 token
        result = send_submit_feedback(url, token=token, data=data)

        print(result)
    return result

submit_feedback(Feedback(
    'test_test',
    '0',
    'problem',
    'contact_phone',
    'company',
    'host_type',
))
