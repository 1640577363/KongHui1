import configparser
import os

# 获取当前脚本所在的路径
current_dir = os.getcwd()
# 获取上一级目录
parent_dir = os.path.dirname(current_dir)
# print("上一级目录:", parent_dir)
config_path=os.path.join(parent_dir,"config","config.ini")
print(config_path)
def load_config(config_file=config_path):
    config = configparser.ConfigParser()
    config.read(config_file)
    return config