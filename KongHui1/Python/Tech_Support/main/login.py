import json

from astropy.extern.ply.yacc import token

from config.read_config import load_config
from request.request import post_request

# 加载配置
config = load_config()
ip = config['remoteHost']['ip']
port = config['remoteHost']['port']
url = f'http://{ip}:{port}/login'

def login(username, password):
    # 登录请求数据
    data = {
        "code": "",
        "password": password,  # 使用传入的密码
        "username": username,  # 使用传入的用户名
        "uuid": ""
    }
    headers = {
        # 'Authorization': f"Bearer {token}",  # 在 Authorization 头部使用 "Bearer" 关键字，通常更常见
        'Content-Type': 'application/json'
    }

    # 发送请求，获取响应
    res = post_request(url, headers, data)

    # 检查响应是否成功
    if res and res.status_code == 200:
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
def save_token_to_config(token, filename='config.json'):
    try:
        # 读取现有配置
        config_data = {}
        try:
            with open(filename, 'r') as f:
                config_data = json.load(f)
        except FileNotFoundError:
            print(f"{filename} 文件不存在，创建新文件。")

        # 更新或添加 token 到配置数据
        config_data['token'] = token

        # 将配置数据写入文件
        with open(filename, 'w') as f:
            json.dump(config_data, f, indent=4)
            print(f"Token 已保存到 {filename}")
    except Exception as e:
        print(f"保存配置文件时出错: {e}")
# 使用示例


def load_token_from_config(filename='config.json'):
    try:
        # 读取配置文件内容
        with open(filename, 'r') as f:
            config_data = json.load(f)

        # 获取 token
        token = config_data.get('token')
        if token:
            return token
        else:
            print("配置文件中没有找到 token")
            return None
    except FileNotFoundError:
        print(f"{filename} 文件未找到")
        return None
    except json.JSONDecodeError:
        print("配置文件的格式无效")
        return None
    except Exception as e:
        print(f"读取配置文件时出错: {e}")
        return None

def get_new_token():

    username = config['user']['name']  # 替换为需要登录的用户名
    password = config['user']['password']  # 替换为需要登录的密码

    response_data = login(username, password)
    if response_data['code']==200:
        # 获取 token
        token = response_data.get('token')
        if token:
            print("Token 获取成功:", token)
            # 将 token 存储到配置文件
            save_token_to_config(token)
            return token
        else:
            print("没有获取到 token")
    else:
        print("登录失败，无法保存 token")