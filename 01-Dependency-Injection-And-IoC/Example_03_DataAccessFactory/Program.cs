using Example_03_DataAccessFactory;

EmployeeBusinessLogic BL = new EmployeeBusinessLogic();
Employee employeeDetails = BL.GetEmployeeDetails(1);

Console.WriteLine();

Console.WriteLine("Employee Details:");
Console.WriteLine("ID : {0}, Name : {1}, Department : {2}, Salary : {3}",
                    employeeDetails.ID, employeeDetails.Name, employeeDetails.Department, employeeDetails.Salary);
Console.ReadKey();