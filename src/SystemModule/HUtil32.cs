using System;
using System.IO;
using System.Text;

namespace SystemModule
{
    public class HUtil32
    {
        private const int V = 0;

        /// <summary>
        /// 根据GUID获取唯一数字序列
        /// </summary>
        public static int Sequence()
        {
            var bytes = Guid.NewGuid().ToByteArray();
            var sequence = BitConverter.ToInt32(bytes, 0);
            while (sequence < 0)
            {
                bytes = Guid.NewGuid().ToByteArray();
                sequence = BitConverter.ToInt32(bytes, 0);
                if (sequence > 0) break;
            }

            return sequence;
        }

        public static int GetTickCount()
        {
            return Environment.TickCount;
        }

        public static int MakeLong(int lowPart, int highPart)
        {
            return lowPart | (short) highPart << 16;
        }

        public static int MakeLong(double lowPart, double highPart)
        {
            return (int) lowPart | ((int) highPart << 16);
        }

        public static int MakeLong(ushort lowPart, int highPart)
        {
            return lowPart | (short) highPart << 16;
        }

        public static int MakeLong(short lowPart, int highPart)
        {
            return (ushort) lowPart | ((short) highPart << 16);
        }

        public static int MakeLong(short lowPart, short highPart)
        {
            return (ushort) lowPart | (highPart << 16);
        }

        public static int MakeLong(short lowPart, ushort highPart)
        {
            return (ushort) lowPart | ((short) highPart << 16);
        }

        //public static ushort MakeWord(byte bLow, byte bHigh)
        //{
        //    return (ushort)(bLow | (bHigh << 8));
        //}

        public static ushort MakeWord(int bLow, int bHigh)
        {
            return (ushort) (bLow | (bHigh << 8));
        }

        public static ushort HiWord(int dword)
        {
            return (ushort) (dword >> 16);
        }

        public static ushort LoWord(int dword)
        {
            return (ushort) dword;
        }

        public static byte HiByte(short W)
        {
            return (byte) (W >> 8);
        }

        public static byte HiByte(int W)
        {
            return (byte) (W >> 8);
        }

        public static byte LoByte(short W)
        {
            return (byte) W;
        }

        public static byte LoByte(int W)
        {
            return (byte) W;
        }

        public static int Round(object r)
        {
            return (int) Math.Round(Convert.ToDouble(r) + 0.5, 1, MidpointRounding.AwayFromZero);
        }

        public static int Round(double r)
        {
            return (int)Math.Round(Convert.ToDouble(r) + 0.5, 1, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// 判断数值是否在范围之内
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static bool RangeInDefined(int values, int min, int max)
        {
            return Math.Max(min, values) == Math.Min(values, max);
        }

        /// <summary>
        /// 判断数值是否在范围之内
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static bool RangeInDefined(long values, int min, int max)
        {
            return Math.Max(min, values) == Math.Min(values, max);
        }

        public static void EnterCriticalSection(object obj)
        {
        }

        public static void LeaveCriticalSection(object obj)
        {
        }

        public static string GetString(byte[] bytes, int index, int count)
        {
            return Encoding.GetEncoding("gb2312").GetString(bytes, index, count);
        }

        public static string StrPas(byte[] buff)
        {
            var nLen = buff.Length;
            var ret = new string('\0', nLen);
            var sb = new StringBuilder(ret);
            for (var i = 0; i < nLen; i++)
            {
                sb[i] = (char)buff[i];
            }
            return sb.ToString();
        }

        public static string StrPasTest(byte[] buff)
        {
            var nLen = 0;
            if (buff[buff.Length - 1] == 0)
            {
                nLen = buff.Length - 1;
            }
            else
            {
                nLen = buff.Length;
            }
            var sb = new char[nLen];
            for (var i = 0; i < nLen; i++)
            {
                sb[i] = (char)buff[i];
            }
            return new string(sb);
        }

        /// <summary>
        /// 字符串转Byte数组
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static unsafe byte[] StringToByteAry(string str)
        {
            var nLen = StringToBytePtr(str, null, 0);
            var ret = new byte[nLen];
            fixed (byte* pb = ret)
            {
                StringToBytePtr(str, pb, 0);
            }
            return ret;
        }

        /// <summary>
        /// 字符串转丹字节
        /// 思路：对于含有高字节不为0的，说明字符串包含汉字，用Encoding.Default.GetBytes
        /// 这样会导致服务端string结构发生变化，但是不影响网络传输的数据
        /// 对于高字节为0的，仅处理低字节
        /// retby 为 null 表示仅计算长度并返回
        /// </summary>
        /// <param name="str"></param>
        /// <param name="retby"></param>
        /// <param name="StartIndex"></param>
        /// <returns></returns>
        public static unsafe int StringToBytePtr(string str, byte* retby, int StartIndex)
        {
            var bDecode = false;
            if (string.IsNullOrEmpty(str)) return 0;
            for (var i = 0; i < str.Length; i++)
                if (str[i] >> 8 != 0)
                {
                    bDecode = true;
                    break;
                }

            var nLen = 0;
            if (bDecode)
                nLen = Encoding.GetEncoding("gb2312").GetByteCount(str);
            else
                nLen = str.Length;
            if (retby == null)
                return nLen;

            if (bDecode)
            {
                var by = Encoding.GetEncoding("gb2312").GetBytes(str);
                var pb = retby + StartIndex;
                for (var i = 0; i < by.Length; i++)
                    *pb++ = by[i];
            }
            else
            {
                var pb = retby + StartIndex;
                for (var i = 0; i < str.Length; i++) *pb++ = (byte) str[i];
            }

            return nLen;
        }

        public static string CaptureString(string source, ref string rdstr)
        {
            string result;
            int st;
            int et;
            int c;
            int len;
            int i;
            if (source == "")
            {
                rdstr = "";
                result = "";
                return result;
            }

            c = 1;
            // et := 0;
            len = source.Length;
            while (source[c] == ' ')
                if (c < len)
                    c++;
                else
                    break;
            if (source[c] == '\"' && c < len)
            {
                st = c + 1;
                et = len;
                for (i = c + 1; i <= len; i++)
                    if (source[i] == '\"')
                    {
                        et = i - 1;
                        break;
                    }
            }
            else
            {
                st = c;
                et = len;
                for (i = c; i <= len; i++)
                    if (source[i] == ' ')
                    {
                        et = i - 1;
                        break;
                    }
            }

            rdstr = source.Substring(st - 1, et - st + 1);
            if (len >= et + 2)
                result = source.Substring(et + 2 - 1, len - (et + 1));
            else
                result = "";
            return result;
        }

        public static string RemoveSpace(string str)
        {
            string result;
            int i;
            result = "";
            for (i = 1; i <= str.Length; i++)
                if (str[i] != ' ')
                    result = result + str[i];
            return result;
        }

        public static int Str_ToInt(string Str, int def)
        {
            var result = def;
            int.TryParse(Str, out result);
            return result;
        }

        public static DateTime Str_ToDate(string Str)
        {
            DateTime result;
            if (Str.Trim() == "")
                result = DateTime.Today;
            else
                result = Convert.ToDateTime(Str);
            return result;
        }

        public static DateTime Str_ToTime(string Str)
        {
            DateTime result;
            if (Str.Trim() == "")
                result = DateTime.Now;
            else
                result = Convert.ToDateTime(Str);
            return result;
        }

        public static double Str_ToFloat(string str)
        {
            double result;
            if (str != "")
                try
                {
                    result = Convert.ToSingle(str);
                    return result;
                }
                catch
                {
                }

            result = 0;
            return result;
        }

        public static string ExtractFileNameOnly(string fname)
        {
            string result;
            int extpos;
            string ext;
            string fn;
            ext = Path.GetExtension(fname);
            fn = Path.GetFileName(fname);
            if (ext != "")
            {
                extpos = fn.IndexOf(ext);
                result = fn.Substring(0, extpos - 1);
            }
            else
            {
                result = fn;
            }

            return result;
        }

        public static string GetValidStr3(string Str, ref string Dest, char Divider)
        {
            var Ary = Str.Split('/'); //返回不包含空的值
            if (Ary.Length > 0)
                Dest = Ary[0]; //目标置为第一个
            else
                Dest = "";
            if (Ary.Length > 1)
                return Ary[1]; //返回第二个
            else
                return "";
        }

        public static string GetValidStr3(string Str, ref string Dest, char[] DividerAry)
        {
            var Div = new char[DividerAry.Length];
            int i;
            for (i = 0; i < DividerAry.Length; i++) Div[i] = DividerAry[i];

            var Ary = Str.Split(Div, 2, StringSplitOptions.RemoveEmptyEntries); //返回不包含空的值
            if (Ary.Length > 0)
                Dest = Ary[0]; //目标置为第一个
            else
                Dest = "";
            if (Ary.Length > 1)
                return Ary[1]; //返回第二个
            else
                return "";
        }

        public static string GetValidStr3(string Str, ref string Dest, string[] DividerAry)
        {
            var Div = new char[DividerAry.Length];
            for (var i = 0; i < DividerAry.Length; i++) Div[i] = DividerAry[i][0];
            var Ary = Str.Split(Div, 2, StringSplitOptions.RemoveEmptyEntries); //返回不包含空的值
            Dest = Ary.Length > 0 ? Ary[0] : "";
            return Ary.Length > 1 ? Ary[1] : "";
        }
        
        public static string GetValidStr3(string Str, ref string Dest, string DividerAry)
        {
            var div = new char[DividerAry.Length];
            for (var i = 0; i < DividerAry.Length; i++) div[i] = DividerAry[i];
            var Ary = Str.Split(div, 2, StringSplitOptions.RemoveEmptyEntries); //返回不包含空的值
            Dest = Ary.Length > 0 ? Ary[0] : "";
            return Ary.Length > 1 ? Ary[1] : "";
        }

        // " " capture => CaptureString (source: string; var rdstr: string): string;
        // ** 贸澜俊 " 绰 亲惑 盖 贸澜俊 乐促绊 啊沥
        public static string GetValidStrCap(string Str, ref string Dest, string[] Divider)
        {
            string result;
            Str = Str.TrimStart();
            if (Str != "")
            {
                if (Str[0] == '\"')
                    result = CaptureString(Str, ref Dest);
                else
                    result = GetValidStr3(Str, ref Dest, Divider);
            }
            else
            {
                result = "";
                Dest = "";
            }

            return result;
        }

        public static bool IsStringNumber(string str)
        {
            var result = true;
            for (var i = 0; i <= str.Length - 1; i++)
                if ((byte) str[i] < (byte) '0' || (byte) str[i] > (byte) '9')
                {
                    result = false;
                    break;
                }

            return result;
        }

        /// <summary>
        /// 截取字符串 例 ArrestStringEx('[1234]','[',']',str)    str=1234
        /// </summary>
        /// <param name="Source">源字符串</param>
        /// <param name="SearchAfter">需要匹配的符号</param>
        /// <param name="ArrestBefore">需要匹配的符号</param>
        /// <param name="ArrestStr">截取之后的结果</param>
        /// <returns></returns>
        public static string ArrestStringEx(string Source, string SearchAfter, string ArrestBefore,
            ref string ArrestStr)
        {
            var result = string.Empty;
            int srclen;
            bool GoodData;
            int n;
            ArrestStr = string.Empty;
            if (Source == "")
            {
                result = "";
                return result;
            }

            try
            {
                srclen = Source.Length;
                GoodData = false;
                if (srclen >= 2)
                {
                    if (Source[0].ToString() == SearchAfter)
                    {
                        Source = Source.Substring(1, srclen - 1);
                        srclen = Source.Length;
                        GoodData = true;
                    }
                    else
                    {
                        n = Source.IndexOf(SearchAfter) + 1;
                        if (n > 0)
                        {
                            Source = Source.Substring(n, srclen - n);
                            srclen = Source.Length;
                            GoodData = true;
                        }
                    }
                }

                if (GoodData)
                {
                    n = Source.IndexOf(ArrestBefore) + 1;
                    if (n > 0)
                    {
                        ArrestStr = Source.Substring(0, n - 1);
                        result = Source.Substring(n, srclen - n);
                    }
                    else
                    {
                        result = SearchAfter + Source;
                    }
                }
                else
                {
                    for (var i = 1; i <= srclen; i++)
                        if (Source[i - 1].ToString() == SearchAfter)
                        {
                            result = Source.Substring(i - 1, srclen - i + 1);
                            break;
                        }
                }
            }
            catch
            {
                ArrestStr = "";
                result = "";
            }

            return result;
        }

        public static string ArrestStringEx(string Source, char SearchAfter, char ArrestBefore, ref string ArrestStr)
        {
            var result = string.Empty;
            int srclen;
            bool GoodData;
            int n;
            ArrestStr = string.Empty;
            if (Source == "")
            {
                result = "";
                return result;
            }

            try
            {
                srclen = Source.Length;
                GoodData = false;
                if (srclen >= 2)
                {
                    if (Source[0].ToString() == SearchAfter.ToString())
                    {
                        Source = Source.Substring(1, srclen - 1);
                        srclen = Source.Length;
                        GoodData = true;
                    }
                    else
                    {
                        n = Source.IndexOf(SearchAfter) + 1;
                        if (n > 0)
                        {
                            Source = Source.Substring(n, srclen - n);
                            srclen = Source.Length;
                            GoodData = true;
                        }
                    }
                }

                if (GoodData)
                {
                    n = Source.IndexOf(ArrestBefore) + 1;
                    if (n > 0)
                    {
                        ArrestStr = Source.Substring(0, n - 1);
                        result = Source.Substring(n, srclen - n);
                    }
                    else
                    {
                        result = SearchAfter + Source;
                    }
                }
                else
                {
                    for (var i = 1; i <= srclen; i++)
                        if (Source[i - 1].ToString() == SearchAfter.ToString())
                        {
                            result = Source.Substring(i - 1, srclen - i + 1);
                            break;
                        }
                }
            }
            catch
            {
                ArrestStr = "";
                result = "";
            }

            return result;
        }

        public static bool CompareLStr(string src, string targ, int compn)
        {
            var result = false;
            if (compn <= 0) return result;
            if (src.Length < compn) return result;
            if (targ.Length < compn) return result;
            result = true;
            for (var i = 0; i <= compn - 1; i++)
            {
                if (char.ToUpper(src[i]) == char.ToUpper(targ[i])) continue;
                result = false;
                break;
            }
            return result;
        }

        private static bool IsEnglish(char Ch)
        {
            var result = false;
            if (Ch >= 'A' && Ch <= 'Z' || Ch >= 'a' && Ch <= 'z') result = true;
            return result;
        }

        public static bool IsEngNumeric(char Ch)
        {
            var result = false;
            if (IsEnglish(Ch) || Ch >= '0' && Ch <= '9') result = true;
            return result;
        }

        public static bool IsEnglishStr(string sEngStr)
        {
            var result = false;
            for (var i = 1; i <= sEngStr.Length; i++)
            {
                result = IsEnglish(sEngStr[i]);
                if (result) break;
            }

            return result;
        }

        public static bool IsFloatNumeric(string str)
        {
            bool result;
            if (str.Trim() == "")
            {
                result = false;
                return result;
            }

            try
            {
                Convert.ToSingle(str);
                result = true;
            }
            catch
            {
                result = false;
            }

            return result;
        }

        public static string ReplaceChar(string src, char srcchr, char repchr)
        {
            string result;
            int i;
            int len;
            if (src != "")
            {
                len = src.Length;
                var sb = new StringBuilder();
                for (i = 0; i < len; i++)
                    if (src[i] == srcchr)
                        sb.Append(repchr);
            }

            result = src;
            return result;
        }

        public static int TagCount(string source, char tag)
        {
            var result = 0;
            var tcount = 0;
            for (var i = 0; i <= source.Length - 1; i++)
                if (source[i] == tag)
                    tcount++;
            result = tcount;
            return result;
        }

        public static string BoolToStr(bool boo)
        {
            string result;
            if (boo)
                result = "TRUE";
            else
                result = "FALSE";
            return result;
        }

        public static int _MIN(int n1, int n2)
        {
            int result;
            if (n1 < n2)
                result = n1;
            else
                result = n2;
            return result;
        }

        public static int _MAX(int n1, int n2)
        {
            int result;
            if (n1 > n2)
                result = n1;
            else
                result = n2;
            return result;
        }

        public static string BoolToCStr(bool b)
        {
            string result;
            if (b)
                result = "是";
            else
                result = "否";
            return result;
        }

        public static string BoolToIntStr(bool b)
        {
            string result;
            if (b)
                result = "1";
            else
                result = "0";
            return result;
        }

        public static byte[] GetBytes(string str)
        {
            return Encoding.GetEncoding("gb2312").GetBytes(str);
        }

        public static byte[] GetBytes(int str)
        {
            return Encoding.GetEncoding("gb2312").GetBytes(str.ToString());
        }

        public static int GetDayCount(DateTime MaxDate, DateTime MinDate)
        {
            int result = V;
            if (MaxDate < MinDate) return result;
            int YearMax = MaxDate.Year;
            int MonthMax = MaxDate.Month;
            int DayMax = MaxDate.Day;
            int YearMin = MinDate.Year;
            int MonthMin = MinDate.Month;
            int DayMin = MinDate.Day;
            YearMax -= YearMin;
            YearMin = 0;
            result = YearMax * 12 * 30 + MonthMax * 30 + DayMax - (YearMin * 12 * 30 + MonthMin * 30 + DayMin);
            return result;
        }

        public static int GetCodeMsgSize(double X)
        {
            int result;
            if (Convert.ToInt32(X) < X)
                result = Convert.ToInt32(X) + 1;
            else
                result = Convert.ToInt32(X);
            return result;
        }

        public static unsafe void IntPtrToIntPtr(IntPtr Src, int SrcIndex, IntPtr Dest, int DestIndex, int nLen)
        {
            var pSrc = (byte*) Src + SrcIndex;
            var pDest = (byte*) Dest + DestIndex;
            if (pDest > pSrc)
            {
                pDest = pDest + (nLen - 1);
                pSrc = pSrc + (nLen - 1);
                for (var i = 0; i < nLen; i++)
                    *pDest-- = *pSrc--;
            }
            else
            {
                for (var i = 0; i < nLen; i++)
                    *pDest++ = *pSrc++;
            }
        }

        /// <summary>
        /// SByte转string
        /// </summary>
        /// <param name="by"></param>
        /// <param name="StartIndex"></param>
        /// <param name="Len"></param>
        /// <returns></returns>
        public static unsafe string SBytePtrToString(sbyte* by, int StartIndex, int Len)
        {
            try
            {
                return BytePtrToString((byte*) by, StartIndex, Len);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        
        public static unsafe string BytePtrToString(byte* by, int StartIndex, int Len)
        {
            var ret = new string('\0', Len);
            var sb = new StringBuilder(ret);

            by += StartIndex;
            for (var i = 0; i < Len; i++) sb[i] = (char) *@by++;

            return sb.ToString();
        }

        /// <summary>
        /// 字符串转Byte字节数组
        /// </summary>
        /// <param name="str"></param>
        /// <param name="strLength"></param>
        /// <returns></returns>
        public static unsafe byte[] StringToByteAry(string str, out int strLength)
        {
            strLength = StringToBytePtr(str, null, 0);
            var ret = new byte[strLength + 1];
            fixed (byte* pb = ret)
            {
                StringToBytePtr(str, pb, 1);
            }

            return ret;
        }

        public static bool CompareBackLStr(string Src, string targ, int compn)
        {
            var result = false;
            int slen;
            int tLen;
            if (compn <= 0)
            {
                return result;
            }
            if (Src.Length < compn)
            {
                return result;
            }
            if (targ.Length < compn)
            {
                return result;
            }
            slen = Src.Length;
            tLen = targ.Length;
            result = true;
            for (var i = 0; i < compn; i++)
            {
                if (char.ToUpper(Src[slen - (i + 1)]) != char.ToUpper(targ[tLen - (i + 1)]))
                {
                    result = false;
                    break;
                }
            }
            return result;
        }

    }
}