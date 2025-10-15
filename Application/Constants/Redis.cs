namespace VelocityAPI.Application.Constants;

public class Redis
{
    public static String RedisInstanceName = "velocityapi:";

    // Cars
    public static String CarUploadPendingKey = "car:upload:pending";
    public static String AuctionLockKey = "car:auction:lock";
    public static String HighestBidKey = "car:highestbid";
}
