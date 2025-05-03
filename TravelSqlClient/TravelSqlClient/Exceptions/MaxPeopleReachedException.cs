namespace TravelSqlClient.Exceptions;

public class MaxPeopleReachedException(string message) : Exception(message);