namespace WorkFlow.Api.Models;

public enum UserRole
{
    Administrator,
    Manager,
    Employee,
    Customer
}

public enum ProjectStatus
{
    Planned,
    Active,
    OnHold,
    Completed
}

public enum WorkTaskStatus
{
    NotStarted,
    InProgress,
    InReview,
    Done
}

public enum TaskPriority
{
    Low,
    Medium,
    High
}