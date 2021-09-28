using System;
namespace Trip.PasvAPI.AppCode
{
    public static class MarketingMapping
    {
         public static string ToKKdayState(string locale)
        {
            var result = "kk";

            switch (locale)
            {
                case "zh-CN": result = "cn"; break;
                case "zh-HK": result = "hk"; break;
                case "zh-TW": result = "tw"; break;
                case "en-HK": result = "hk"; break;
                case "en-MY": result = "my"; break;
                case "en-SG": result = "sg"; break; 
                case "ko-KR": result = "kr"; break;
                case "ja-JP": result = "jp"; break;
                case "th-TH": result = "th"; break;
                case "en-AU":
                case "en-GB": 
                case "en-US":
                case "en-XX":
                default: break;
            }

            return result;
        }
    }
}
