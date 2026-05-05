using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIP_Example_02_bad
{
    public class EmployeeStatistics
    {
        private readonly EmployeeManager _empManager;

        public EmployeeStatistics(EmployeeManager empManager)
        {
            _empManager = empManager;
        }

        public int CountFemaleManagers() =>
              _empManager.Employees.Count(emp => emp.Gender == Gender.Female && emp.Position == Position.Manager);
    }
}
