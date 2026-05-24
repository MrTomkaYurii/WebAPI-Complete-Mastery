using AutoMapper;
using Automapper_Example_01;



//// ==1== EXAMPLE WITHOUT AUTOMAPPER
//Student std = new Student();
//std.FullName = "Shura";
//std.Address = "USA";
//std.Department = "CS";
//StudentDTO stdDTO = new StudentDTO();
//stdDTO.FullName = std.FullName;
//stdDTO.Address = std.Address;
//stdDTO.Department = std.Department;
//Console.WriteLine("Name:" + stdDTO.FullName + ", Address:" + stdDTO.Address + ", Department:" + stdDTO.Department);
//Console.ReadLine();



//// ==2== EXAMPLE WITH AUTOMAPPER
////Initialize the mapper
//var config = new MapperConfiguration(cfg =>
//        cfg.CreateMap<Student, StudentDTO>()
//    );
////Creating the source object
//Student std = new Student();
//std.FullName = "Shayan";
//std.Address = "USA";
//std.Department = "CS";
////Using automapper
//var mapper = new Mapper(config);
//var stdDTO = mapper.Map<StudentDTO>(std);
//Console.WriteLine("Name:" + stdDTO.FullName + ", Address:" + stdDTO.Address + ", Department:" + stdDTO.Department);
//Console.ReadLine();



// ==3==
//Initialize the mapper
var config = new MapperConfiguration(cfg =>
        cfg.CreateMap<Student, StudentDTO>()
        .ForMember(des => des.Name, a => a.MapFrom(source => source.FullName))
        .ForMember(des => des.Dept, a => a.MapFrom(source => source.Department))
    );

//Creating the source object
Student std = new Student();
std.FullName = "Mykola";
std.Address = "USA";
std.Department = "CS";

//Using automapper
var mapper = new Mapper(config);
var stdDTO = mapper.Map<StudentDTO>(std);


Console.WriteLine("Name:" + stdDTO.Name + ", Address:" + stdDTO.Address + ", Department:" + stdDTO.Dept);
Console.ReadLine();