using Python.Runtime;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace PythonDemo
{
    class Program
    {
        private static bool _pythonInitialized = false;

        static void Main(string[] args)
        {
            try
            {
                InitializeEmbeddedPython();

                //调用Python脚本中的方法
                CallPythonScript();

                Console.WriteLine("\n按任意键退出...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"错误: {ex.Message}");
                Console.WriteLine($"详细信息: {ex.StackTrace}");
                Console.ReadKey();
            }
            finally
            {
                // 清理Python环境
                if (_pythonInitialized)
                {
                    PythonEngine.Shutdown();//在csproj中添加 <EnableUnsafeBinaryFormatterSerialization>true</EnableUnsafeBinaryFormatterSerialization>
                }
            }
        }

        /// <summary>
        /// 初始化嵌入式Python环境
        /// </summary>
        static void InitializeEmbeddedPython()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            string pythonDir = Path.Combine(baseDir, "python");
            string pythonDll = Path.Combine(pythonDir, "python313.dll"); //bin/python/python313.dll

            // 检查Python DLL是否存在
            if (!File.Exists(pythonDll))
            {
                throw new FileNotFoundException($"找不到Python DLL: {pythonDll}");
            }

            // 设置Python DLL路径
            Runtime.PythonDLL = pythonDll;

            // 设置Python环境变量
            Environment.SetEnvironmentVariable("PYTHONHOME", pythonDir);
            string pythonPath = Path.Combine(pythonDir, "Lib", "site-packages");
            if (Directory.Exists(pythonPath))
            {
                Environment.SetEnvironmentVariable("PYTHONPATH", pythonPath);
            }

            try
            {
                // 初始化Python引擎
                PythonEngine.Initialize();
                _pythonInitialized = true;
                Console.WriteLine("初始化Python引擎成功");
            }
            catch(Exception ex)
            {
                Console.WriteLine($"初始化Python引擎失败");
                throw;
            }

        }

        /// <summary>
        /// 调用Python脚本中的方法
        /// </summary>
        static void CallPythonScript()
        {
            using (Py.GIL()) // 必须获取GIL锁
            {
                // 1. 添加Python脚本路径到sys.path
                dynamic sys = Py.Import("sys");
                string scriptsPath = AppDomain.CurrentDomain.BaseDirectory;

                // 如果脚本在子文件夹中，可以添加对应路径
                sys.path.append(scriptsPath);

                Console.WriteLine($"✓ 脚本搜索路径: {scriptsPath}");
                Console.WriteLine();

                // 2. 导入greet.py模块（注意：不加.py后缀）
                dynamic greetModule;
                try
                {
                    greetModule = Py.Import("greet");
                    Console.WriteLine("✓ 成功导入 greet.py 模块");
                }
                catch (PythonException ex)
                {
                    Console.WriteLine($"✗ 导入模块失败: {ex.Message}");
                    throw;
                }

                // 3. 调用Python方法示例
                Console.WriteLine("\n--- 开始调用Python方法 ---\n");

                // 示例1：调用 say_hello 方法
                try
                {
                    string name = "张三";
                    dynamic result = greetModule.say_hello(name);
                    string message = result.As<string>();
                    Console.WriteLine($"调用 say_hello('{name}') → {message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"调用say_hello失败: {ex.Message}");
                }

                // 示例2：调用其他可能的方法（根据你的greet.py实际内容）
                try
                {
                    // 如果greet.py中有add方法，可以调用
                    if (HasAttribute(greetModule, "add"))
                    {
                        int sum = greetModule.add(10, 20);
                        Console.WriteLine($"调用 add(10, 20) → {sum}");
                    }
                }
                catch (Exception ex)
                {
                    // 方法不存在，忽略
                }

                // 示例3：调用带返回值的复杂方法
                try
                {
                    if (HasAttribute(greetModule, "format_greeting"))
                    {
                        dynamic formatted = greetModule.format_greeting("王五", "下午好");
                        Console.WriteLine($"调用 format_greeting → {formatted}");
                    }
                }
                catch (Exception ex)
                {
                    // 方法不存在，忽略
                }
            }
        }

        /// <summary>
        /// 检查Python对象是否有指定的属性/方法
        /// </summary>
        static bool HasAttribute(dynamic obj, string attrName)
        {
            try
            {
                var attr = obj.GetAttr(attrName);
                return attr != null;
            }
            catch
            {
                return false;
            }
        }
    }
}