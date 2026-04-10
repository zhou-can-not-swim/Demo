# greet.py

def say_hello(name):
    """简单的问候函数"""
    return f"你好, {name}!"

def add(a, b):
    """加法函数示例"""
    return a + b

def format_greeting(name, time_of_day):
    """带格式的问候函数"""
    return f"{time_of_day}，{name}！今天过得怎么样？"

# 可选：添加一些测试代码
if __name__ == "__main__":
    # 这个代码只在直接运行Python脚本时执行，被C#导入时不会执行
    print("测试 say_hello:", say_hello("测试用户"))
    print("测试 add:", add(5, 3))