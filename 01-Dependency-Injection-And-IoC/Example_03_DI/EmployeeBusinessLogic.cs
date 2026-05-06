using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example_03_DI
{
    public class EmployeeBusinessLogic
    {
        IEmployeeDataAccess _EmployeeDataAccess;
        public EmployeeBusinessLogic(IEmployeeDataAccess dataAccess)
        {
            _EmployeeDataAccess = dataAccess;
        }
        public Employee GetEmployeeDetails(int id)
        {
            return _EmployeeDataAccess.GetEmployeeDetails(id);
        }
    }

    // Property Injection
    //public class EmployeeBusinessLogic
    //{
    //    public IEmployeeDataAccess EmpDataAccess { get; set; }
    //    public Employee GetEmployeeDetails(int id)
    //    {
    //        return EmpDataAccess.GetEmployeeDetails(id);
    //    }
    //}

    //Method Injection
    //interface IEmployeeDataAccessDependency
    //{
    //    void SetDependency(IEmployeeDataAccess employeeDataAccess);
    //}

    //public class EmployeeBusinessLogic : IEmployeeDataAccessDependency
    //{
    //    public IEmployeeDataAccess EmpDataAccess { get; set; }
    //    public Employee GetEmployeeDetails(int id)
    //    {
    //        return EmpDataAccess.GetEmployeeDetails(id);
    //    }
    //    public void SetDependency(IEmployeeDataAccess employeeDataAccess)
    //    {
    //        EmpDataAccess = employeeDataAccess;
    //    }
    //}


}
