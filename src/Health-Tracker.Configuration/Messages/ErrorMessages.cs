
namespace Health_Tracker.Configuration.Messages;

public static class ErrorMessages
{
	public static class Generic
	{
		public static string SomethingWentWrong = "Something went wrong. Please try again later";
		public static string UnableToProcess = "Unable to process request";
		public static string BadRequest = "Bad Request";
		public static string InvalidPayload = "Invalid Payload";
		public static string InvalidRequest = "Invalid Request";
		public static string DataNotFound = "Data Not Found";
	}

	public static class Profile
	{
		public static string UserNotFound = "User not found";
	}

	public static class Users
	{
		public static string UserNotFound = "User not found";
	}
}
