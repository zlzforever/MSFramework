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
        /// 该方法在栈上复制字符串副本后改写，不会原地修改字符串对象，
        /// 避免破坏 .NET 字符串驻留机制；空串或 null 直接原样返回。
        /// </summary>
        /// <returns>驼峰命名字符串（新实例），null 或空白字符串原样返回</returns>
        public string ToCamelCase()
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            var span = value.AsSpan();
            Span<char> buffer = stackalloc char[span.Length];
            span.CopyTo(buffer);
            buffer[0] = char.ToLowerInvariant(buffer[0]);
            return new string(buffer);
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
