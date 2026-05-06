using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIP_Example_02_bad
{
    public class EmployeeManager
    {
        private readonly List<Employee> _employees;
        public EmployeeManager()
        {
            _employees = new List<Employee>();
        }
        public void AddEmployee(IEnumerable<Employee> employees)
        {
            _employees.AddRange(employees);
        }

        public List<Employee> Employees => _employees;
    }
}
