using Bogus;
using DIP_Example_02_good;

Console.WriteLine("---");

var employeeFaker = new Faker<Employee>()
    .StrictMode(true)
    .RuleFor(emp => emp.Name, f => f.Name.FirstName())
    .RuleFor(emp => emp.Gender, f => f.PickRandom<Gender>())
    .RuleFor(emp => emp.Position, f => f.PickRandom<Position>());

List<Employee> employees = employeeFaker.Generate(20);

Console.WriteLine("---");
void Print(Employee emp)
{
    Console.WriteLine(emp.Name + " " + emp.Gender + " " + emp.Position);
}

employees.ForEach(Print);
Console.WriteLine("---");


EmployeeManager employeeManager = new EmployeeManager();
employeeManager.AddEmployee(employees);

EmployeeStatistics es = new EmployeeStatistics(employeeManager);
Console.WriteLine("Загальна кількість жінок менеджерів = " + es.CountFemaleManagers());