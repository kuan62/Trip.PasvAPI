using System;
namespace Trip.PasvAPI.AppCode
{
    public static class LocaleMapping
    {
        public static string ToKKdayLocale(string locale)
        {
            var result = "en-kk";

            switch(locale)
            {
                case "zh-CN": result = "zh-cn"; break;
                case "zh-HK": result = "zh-hk"; break;
                case "zh-TW": result = "zh-tw"; break;
                case "en-AU": result = "en-us"; break;
                case "en-GB": result = "en-us"; break;
                case "en-HK": result = "en-us"; break;
                case "en-MY": result = "en-my"; break;
                case "en-SG": result = "en-sg"; break;
                case "en-US": result = "en-us"; break;
                case "en-XX": result = "en-kk"; break;
                case "ko-KR": result = "ko-kr"; break;
                case "ja-JP": result = "ja-jp"; break;
                case "th-TH": result = "th-th"; break; 
                default: break;
            }

            return result;
        } 
    }
}
