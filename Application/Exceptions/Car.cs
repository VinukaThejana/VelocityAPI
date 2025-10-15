namespace VelocityAPI.Application.Exceptions.Car;

public class BidTooLowException : Exception
{
    public BidTooLowException(string message) : base(message) { }
}

public class AuctionNotActiveException : Exception
{
    public AuctionNotActiveException(string message) : base(message) { }
}
