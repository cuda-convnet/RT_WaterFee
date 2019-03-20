using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using System.IO;

namespace WHC.Framework.Commons
{
    /// <summary>
    /// �����в�������������
    /// </summary>
    public static class ParamParser
    {
        /// <summary>
        /// ��̬��ʼ������
        /// </summary>
        static ParamParser()
        {
            string[] cmdLineArgs = Environment.GetCommandLineArgs();

            _appFullName = cmdLineArgs[0];
            if (Path.IsPathRooted(_appFullName) == false)
            {
                _appFullName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _appFullName);
            }
            _appName = Path.GetFileNameWithoutExtension(_appFullName);

            string[] args = new string[cmdLineArgs.Length - 1];
            Array.Copy(cmdLineArgs, 1, args, 0, args.Length);

            _switches = new StringDictionary();
            _parameters = new StringCollection();

            Parse(args);
        }

        /// <summary>
        /// �ڲ�ת������
        /// </summary>
        /// <param name="args"></param>
        private static void Parse(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                string currParam = args[i].ToLower();
                if (currParam.StartsWith("/") || currParam.StartsWith("-"))
                {
                    if (currParam.Length >= 2)
                    {
                        string currSwitch = currParam.Substring(1);
                        int switchValueStartIndex = currSwitch.IndexOf(":");

                        string switchKey = switchValueStartIndex > 0 ?
                            currSwitch.Substring(0, switchValueStartIndex) : currSwitch;

                        string switchValue = switchValueStartIndex > 0 ?
                            currSwitch.Substring(switchValueStartIndex + 1) : null;

                        if (switchValue != null)
                            switchValue = switchValue.Trim('\"');
                        else
                            switchValue = string.Empty;

                        _switches.Add(switchKey.ToLower(), switchValue.ToLower());
                    }
                    else
                    {
                        throw new ArgumentException();
                    }
                }
                else
                {
                    _parameters.Add(currParam.ToLower());
                }
            }
        }

        private static string _appFullName;
        /// <summary>
        /// ��ȡ���������ȫ·������
        /// </summary>
        public static string AppFullName
        {
            get { return _appFullName; }
        }

        private static string _appName;
        /// <summary>
        /// ��ȡ��������Ķ�����
        /// </summary>
        public static string AppName
        {
            get { return _appName; }
        }


        private static StringDictionary _switches;
        /// <summary>
        /// �������������Ƿ���������Ŀ��ؼ�
        /// </summary>
        /// <param name="switchKey">��Ҫ���ԵĿ��ؼ�</param>
        /// <returns>�������������������Ŀ��ؼ��򷵻�true�����򷵻�false</returns>
        public static bool ContainSwitch(string switchKey)
        {
            return _switches.ContainsKey(switchKey.ToLower());
        }

        /// <summary>
        /// ��ȡ���������и������ؼ���Ӧ��ֵ
        /// </summary>
        /// <param name="switchKey">��Ҫ����ֵ�Ŀ��ؼ�</param>
        /// <returns>�������������������Ŀ��ؼ��򷵻ظü���Ӧ��ֵ�����򷵻�null</returns>
        public static string GetSwitchValue(string switchKey)
        {
            switchKey = switchKey.ToLower();
            if(_switches.ContainsKey(switchKey))
                return _switches[switchKey];
            return null;
        }

        private static StringCollection _parameters;
        /// <summary>
        /// ��ȡ�����������Ĳ���
        /// </summary>
        /// <param name="index">����������ֵ</param>
        /// <returns>�������������Χ�򷵻�string.Empty�����򷵻ظ����������Ĳ���ֵ</returns>
        public static string GetParameter(int index)
        {
            if (index >= _parameters.Count) return string.Empty;

            return _parameters[index];
        }
    }

    /// <summary>
    /// ��ʽ�����֧�ֵ����
    /// </summary>
    public class HelpPrinter
    {
        private string _description;
        private string _pattern;
        private int _patternPadding;
        private NameValueCollection _comment;

        /// <summary>
        /// ���캯��
        /// </summary>
        /// <param name="description">�Ը�����������ܵ�˵��</param>
        /// <param name="pattern">�ܵ�����������ģʽ</param>
        public HelpPrinter(string description, string pattern)
        {
            _description = description;
            _pattern = pattern;
            _patternPadding = 16;
            _comment = new NameValueCollection();
        }

        /// <summary>
        /// ģʽ��ģʽ˵����Ŀհײ������ֵ
        /// </summary>
        public int PatternPadding
        {
            get { return _patternPadding; }
            set { _patternPadding = value; }
        }

        /// <summary>
        /// ���һ�������˵��
        /// </summary>
        /// <param name="param">��������</param>
        /// <param name="descrption">�ò�������ϸ˵��</param>
        public void AddComment(string param, string descrption)
        {
            _comment.Add(param, descrption);
        }

        /// <summary>
        /// ��Console��ӡ����ʽ�������˵��
        /// </summary>
        /// <returns></returns>
        public string Print()
        {
            StringBuilder sb = new StringBuilder();

            using (StringWriter writer = new StringWriter(sb))
            {
                writer.WriteLine(_description);
                writer.WriteLine();
                writer.WriteLine(_pattern);
                writer.WriteLine();

                string patternFormat = "  {0,-" + _patternPadding + "}";
                for (int i = 0; i < _comment.Count; i++)
                {
                    string key = _comment.GetKey(i);
                    writer.Write(patternFormat, key);

                    string[] comments = _comment.GetValues(i);
                    for (int j = 0; j < comments.Length; j++)
                    {
                        if ((j > 0) && (j % 2 == 0))
                        {
                            writer.WriteLine();
                            writer.Write(new string(' ', 18));
                        }
                        writer.Write("{0,-24}", comments[j]);
                    }
                    writer.WriteLine();
                }
                writer.WriteLine();
                writer.Flush();
            }
            return sb.ToString();
        }
    }
}
