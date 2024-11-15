using System;
using System.Collections.Generic;
using System.Linq;

abstract class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }

    public abstract void DisplayInfo();
}

interface ISalable
{
    void AddSale(decimal saleAmount);
    List<decimal> GetSales();
}

interface IPayment
{
    decimal GetTotalPayments();
    void AddPayment(decimal payment);
}

interface ISearchable
{
    void Search(string query);
}

class Customer : Person, ISalable, IPayment
{
    public List<decimal> Sales { get; set; } = new List<decimal>(); //private

    public Customer(string firstName, string lastName, string email)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
    }

    public override void DisplayInfo()
    {
        foreach(var i in Sales)
        {
            Console.WriteLine($"Customer: {FirstName} {LastName}, Email: {Email}, Sales: {i}");
        }
        System.Console.WriteLine();
    }

    void ISalable.AddSale(decimal saleAmount)
    {
        Sales.Add(saleAmount);
    }

    List<decimal> ISalable.GetSales()
    {
        return Sales;
    }

    public decimal GetTotalPayments()
    {
        return Sales.Sum();
    }

    public void AddPayment(decimal payment)
    {
        Sales.Add(payment);
    }

    public override string ToString()
    {
        return $"{FirstName} {LastName} - {Email} | Total Sales: {Sales.Count}, Total Payments: {GetTotalPayments():C}";
    }
}

class Employee : Person
{
    public decimal Salary { get; set; }

    public Employee(string firstName, string lastName, string email, decimal salary)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Salary = salary;
    }

    public override void DisplayInfo()
    {
        Console.WriteLine($"Employee: {FirstName} {LastName}, Email: {Email}, Salary: {Salary:C}");
    }

    public override string ToString()
    {
        return $"{FirstName} {LastName} - {Email} | Salary: {Salary:C}";
    }
}

class CRMSystem : ISearchable
{
    private List<Customer> customers = new();
    private List<Employee> employees = new();

    public void AddCustomer(Customer customer)
    {
        if (IsEmailDuplicate(customer.Email))
        {
            throw new DuplicateEmailException("This email is already in use.");
        }
        customers.Add(customer);
    }

    public void AddEmployee(Employee employee)
    {
        if (IsEmailDuplicate(employee.Email))
        {
            throw new DuplicateEmailException("This email is already in use.");
        }
        employees.Add(employee);
    }

    public void DisplayAllCustomers()
    {
        if (customers.Count == 0)
        {
            Console.WriteLine("No customers found.");
        }
        else
        {
            foreach (var customer in customers)
            {
                customer.DisplayInfo();
            }
        }
    }

    public void DisplayAllEmployees()
    {
        if (employees.Count == 0)
        {
            Console.WriteLine("No employees found.");
        }
        else
        {
            foreach (var employee in employees)
            {
                employee.DisplayInfo();
            }
        }
    }

    public void SearchCustomerByName(string name)
    {
        var foundCustomers = customers.FindAll(c => c.FirstName.Contains(name, StringComparison.OrdinalIgnoreCase) || c.Email.Contains(name , StringComparison.OrdinalIgnoreCase) || c.LastName.Contains(name, StringComparison.OrdinalIgnoreCase));
        if (foundCustomers.Count == 0)
        {
            Console.WriteLine("No customers found.");
        }
        else
        {
            foreach (var customer in foundCustomers)
            {
                Console.WriteLine(customer);
            }
        }
    }

    public void Search(string query)
    {
        SearchCustomerByName(query);
    }

    public bool IsEmailDuplicate(string email)
    {
        return customers.Exists(c => c.Email.Equals(email, StringComparison.OrdinalIgnoreCase)) ||
               employees.Exists(e => e.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    }
}

public class DuplicateEmailException : Exception
{
    public DuplicateEmailException(string message) : base(message) { }
}

class Program
{
    static void Main(string[] args)
    {
        CRMSystem crm = new();

        while (true)
        {
            try
            {
                Console.WriteLine("1. Add Customer");
                Console.WriteLine("2. Add Employee");
                Console.WriteLine("3. Display All Customers");
                Console.WriteLine("4. Display All Employees");
                Console.WriteLine("5. Search Customer by Name");
                Console.WriteLine("0. Exit");
                Console.Write("Choose an option: ");
                string choice = Console.ReadLine()!;

                switch (choice)
                {
                    case "1":
                        string firstName = GetValidInput("Enter first name: ");
                        string lastName = GetValidInput("Enter last name: ");
                        string email = GetValidEmail("Enter email: ", crm);

                        Customer customer = new Customer(firstName, lastName, email);
                        crm.AddCustomer(customer);

                        decimal paymentAmount = GetValidDecimalInput("Enter payment amount: ");
                        customer.AddPayment(paymentAmount);

                        Console.WriteLine("Customer added successfully!");
                        Console.WriteLine();
                        break;
                    case "2":
                        firstName = GetValidInput("Enter first name: ");
                        lastName = GetValidInput("Enter last name: ");
                        email = GetValidEmail("Enter email: ", crm);
                        decimal salary = GetValidDecimalInput("Enter salary: ");
                        crm.AddEmployee(new Employee(firstName, lastName, email, salary));
                        Console.WriteLine("Employee added successfully!");
                        Console.WriteLine();
                        break;
                    case "3":
                        crm.DisplayAllCustomers();
                        break;
                    case "4":
                        crm.DisplayAllEmployees();
                        break;
                    case "5":
                        string query = GetValidInput("Enter the first name, last name, or email to search: ");
                        crm.SearchCustomerByName(query);
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
            }
            catch (DuplicateEmailException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }
    }

    static string GetValidInput(string prompt)
    {
        string input;
        do
        {
            Console.Write(prompt);
            input = Console.ReadLine()!;
            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("Input cannot be empty. Please try again.");
            }
        } while (string.IsNullOrWhiteSpace(input));
        return input;
    }

    static string GetValidEmail(string prompt, CRMSystem crm)
    {
        string email;
        do
        {
            Console.Write(prompt);
            email = Console.ReadLine()!;
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@") || crm.IsEmailDuplicate(email))
            {
                Console.WriteLine("Invalid or duplicate email. Please enter a valid, unique email.");
                email = "";
            }
        } while (string.IsNullOrWhiteSpace(email));
        return email;
    }

    static decimal GetValidDecimalInput(string prompt)
    {
        decimal result;
        do
        {
            Console.Write(prompt);
            if (!decimal.TryParse(Console.ReadLine(), out result) || result <= 0)
            {
                Console.WriteLine("Invalid amount. Please enter a valid number greater than 0.");
            }
        } while (result <= 0);
        return result;
    }
}
