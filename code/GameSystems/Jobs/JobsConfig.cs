public static class JobsConfig
{
    public static List<Job> Jobs = new() {
        new(){
            Name = "Citizen",
            Description = "Be a normal person.",
            Salary = 0f,
        },
        new(){
            Name = "Cop",
            Description = "Keep the peace in the city, respond to emergencies, and maintain law and order.",
            Salary = 100f,
        },
        new(){
            Name = "Doctor",
            Description = "Heal people in need.",
        },
        new(){
            Name = "Firefighter",
            Description = "Keep the peace in the city, respond to emergencies, and maintain law and order.",
            Salary = 100f,
        }
    };
}