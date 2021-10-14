using System;
using Trip.PasvAPI.Models.Enum;

namespace Trip.PasvAPI.AppCode
{
    public static class TripOrderStatusMapping
    {
        public static int Convert(string status)
        {
            int result = -1;

            switch(status)
            {
                case "NW":
                case "GO":
                    result = (int)TTdOrderStatusEnum.NEW_WAIT;
                    break;
                case "GO_OK":
                    result = (int)TTdOrderStatusEnum.NEW_CONFIRMED;
                    break;
                case "CX_ING":
                    result = (int)TTdOrderStatusEnum.CANCEL_APPLY;
                    break;
                case "CX":
                    result = (int)TTdOrderStatusEnum.ALL_CANCELED;
                    break;
                default:
                    result = -1;
                    break;
            }

            return result;
        }
    }
}
