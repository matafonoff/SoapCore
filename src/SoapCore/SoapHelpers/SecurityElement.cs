using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace SoapCore
{
	//original: https://raw.githubusercontent.com/dotnet/corefx/master/src/System.Runtime.Extensions/src/System/Security/SecurityElement.cs
	public class SecurityElement
	{
		private static readonly string[] s_escapeStringPairs =
		{
			// these must be all once character escape sequences or a new escaping algorithm is needed
			"<",
			"&lt;",
			">",
			"&gt;",
			"\"",
			"&quot;",
			"\'",
			"&apos;",
			"&",
			"&amp;"
		};

		private static readonly char[] s_escapeChars = {'<', '>', '\"', '\'', '&'};

		public static string Escape(string str)
		{
			if (str == null)
			{
				return null;
			}

			StringBuilder sb = null;

			var strLen = str.Length;
			var newIndex = 0; // Pointer into the string that indicates the start index of the "remaining" string (that still needs to be processed).

			while (true)
			{
				var index = str.IndexOfAny(s_escapeChars, newIndex); // Pointer into the string that indicates the location of the current '&' character

				if (index == -1)
				{
					if (sb == null)
					{
						return str;
					}

					sb.Append(str, newIndex, strLen - newIndex);
					return sb.ToString();
				}

				if (sb == null)
				{
					sb = new StringBuilder();
				}

				sb.Append(str, newIndex, index - newIndex);
				sb.Append(GetEscapeSequence(str[index]));

				newIndex = index + 1;
			}

			// no normal exit is possible
		}

		[MethodImpl(MethodImplOptions.NoOptimization)]
		public static void Init()
		{ }

		private static string GetEscapeSequence(char c)
		{
			var iMax = s_escapeStringPairs.Length;
			Debug.Assert(iMax % 2 == 0, "Odd number of strings means the attr/value pairs were not added correctly");

			for (var i = 0; i < iMax; i += 2)
			{
				var strEscSeq = s_escapeStringPairs[i];
				var strEscValue = s_escapeStringPairs[i + 1];

				if (strEscSeq[0] == c)
				{
					return strEscValue;
				}
			}

			Debug.Fail("Unable to find escape sequence for this character");
			return c.ToString();
		}
	}

	public class SecurityElement2
	{
		private static readonly Dictionary<char, string> s_escapeStringPairs = new Dictionary<char, string>
		{
			// these must be all once character escape sequences or a new escaping algorithm is needed
			{'<', "&lt;"},
			{'>', "&gt;"},
			{'\"', "&quot;"},
			{'\'', "&apos;"},
			{'&', "&amp;"}
		};

		private static readonly char[] s_escapeChars = s_escapeStringPairs.Keys.ToArray();

		[MethodImpl(MethodImplOptions.NoOptimization)]
		public static void Init()
		{ }

		public static string Escape(string str)
		{
			if (str == null)
			{
				return null;
			}

			StringBuilder sb = null;

			var strLen = str.Length;
			var newIndex = 0; // Pointer into the string that indicates the start index of the "remaining" string (that still needs to be processed).

			while (true)
			{
				var index = str.IndexOfAny(s_escapeChars, newIndex); // Pointer into the string that indicates the location of the current '&' character

				if (index == -1)
				{
					if (sb == null)
					{
						return str;
					}

					sb.Append(str, newIndex, strLen - newIndex);
					return sb.ToString();
				}

				if (sb == null)
				{
					sb = new StringBuilder();
				}

				sb.Append(str, newIndex, index - newIndex);
				sb.Append(GetEscapeSequence(str[index]));

				newIndex = index + 1;
			}

			// no normal exit is possible
		}

		private static string GetEscapeSequence(char c)
		{
			return s_escapeStringPairs.TryGetValue(c, out var str) ? str : c.ToString();
		}
	}
}
