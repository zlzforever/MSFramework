using System;
using System.Globalization;
using System.Text;

namespace MicroserviceFramework.Runtime;

/// <summary>
/// 字符串扩展方法
/// </summary>
public static class StringExtensions
{
    // public static string ToHex(this IEnumerable<byte> bytes)
    // {
    //     var builder = new StringBuilder();
    //     foreach (var b in bytes)
    //     {
    //         builder.Append($"{b:x2}");
    //     }
    //
    //     return builder.ToString();
    // }

    /// <param name="value"></param>
    extension(string value)
    {
        /// <summary>
        /// 判断字符串是否为 null 或空字符串
        /// </summary>
        /// <returns>如果是 null 或空返回 true</returns>
        public bool IsNullOrEmpty()
        {
            return string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// 判断字符串是否为 null 或空白
        /// </summary>
        /// <returns>如果是 null 或空白返回 true</returns>
        public bool IsNullOrWhiteSpace()
        {
            return string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// 将字符串转换为蛇形命名（snake_case）
        /// </summary>
        /// <returns>蛇形命名字符串</returns>
        public string ToSnakeCase() => ToSeparatedCase(value, '_');

        /// <summary>
        /// 将字符串转换为驼峰命名（camelCase），首字符转换为小写。
        /// 该方法在堆上生成字符串副本后改写，不会原地修改字符串对象，
        /// 避免破坏 .NET 字符串驻留机制；空串或 null 直接原样返回。
        /// </summary>
        /// <remarks>
        /// 不使用 <c>stackalloc char[len]</c>：长度来自字符串本身，而本方法的部分调用点
        /// （如模型绑定错误键）传入了客户端可控的超长字符串，栈分配超过线程栈上限时触发
        /// 不可捕获的 <see cref="StackOverflowException"/> 直接终止进程（DoS）。
        /// <see cref="System.Text.Json.JsonNamingPolicy.CamelCase"/> 的 BCL 实现对首两个字符
        /// 均为大写的名称（如 URLValue）有保留原样的缩写词处理，语义与本方法
        /// （无条件小写首字符）不同，替换会改变现有行为，故仍保留自有实现。
        /// </remarks>
        /// <returns>驼峰命名字符串（新实例），null 或空白字符串原样返回</returns>
        public string ToCamelCase()
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
            var span = value.AsSpan();
            // 硬阈值，例如 128，超过就走 string.Create 兜底
            const int StackAllocThreshold = 128;
            if (span.Length <= StackAllocThreshold)
            {
                Span<char> buffer = stackalloc char[span.Length];
                span.CopyTo(buffer);
                buffer[0] = char.ToLowerInvariant(buffer[0]);
                return new string(buffer);
            }
            // 超长走安全路径
            return string.Create(span.Length, span, static (buf, src) =>
            {
                src.CopyTo(buf);
                buf[0] = char.ToLowerInvariant(buf[0]);
            });
        }
    }

    private enum SeparatedCaseState
    {
        Start,
        Lower,
        Upper,
        NewWord,
    }

    private static string ToSeparatedCase(string s, char separator)
    {
        if (string.IsNullOrEmpty(s))
            return s;
        var stringBuilder = new StringBuilder();
        var separatedCaseState = SeparatedCaseState.Start;
        for (var index = 0; index < s.Length; ++index)
        {
            if (s[index] == ' ')
            {
                if (separatedCaseState != SeparatedCaseState.Start)
                    separatedCaseState = SeparatedCaseState.NewWord;
            }
            else if (char.IsUpper(s[index]))
            {
                switch (separatedCaseState)
                {
                    case SeparatedCaseState.Lower:
                    case SeparatedCaseState.NewWord:
                        stringBuilder.Append(separator);
                        break;
                    case SeparatedCaseState.Upper:
                        var flag = index + 1 < s.Length;
                        if (index > 0 & flag)
                        {
                            var c = s[index + 1];
                            if (!char.IsUpper(c) && c != separator)
                            {
                                stringBuilder.Append(separator);
                            }
                        }

                        break;
                }

                var lower = char.ToLower(s[index], CultureInfo.InvariantCulture);
                stringBuilder.Append(lower);
                separatedCaseState = SeparatedCaseState.Upper;
            }
            else if (s[index] == separator)
            {
                stringBuilder.Append(separator);
                separatedCaseState = SeparatedCaseState.Start;
            }
            else
            {
                if (separatedCaseState == SeparatedCaseState.NewWord)
                    stringBuilder.Append(separator);
                stringBuilder.Append(s[index]);
                separatedCaseState = SeparatedCaseState.Lower;
            }
        }

        return stringBuilder.ToString();
    }
}
