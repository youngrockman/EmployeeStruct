using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Interactivity;
using EmployeeStruct.Models;

namespace EmployeeStruct;

public partial class EmployeeEditWindow : Window
{
    private readonly Employee _employee;
    private bool _isEditing;
    private readonly User15Context _db;
    public Employee Employee { get; private set; } 
    public EmployeeEditWindow()
    {
        InitializeComponent();
        _employee = new Employee();
        _db = new User15Context();
        LoadData();
        SetEditMode(false);
    }

    private void LoadData()
    {
  
        var departmentsComboBox = this.FindControl<ComboBox>("DepartmentsComboBox");
        departmentsComboBox.ItemsSource = _db.DepartmentSubdivisions.ToList();
        
 
        var positionsComboBox = this.FindControl<ComboBox>("PositionsComboBox");
        positionsComboBox.ItemsSource = _db.Positions.ToList();
        
        
        var assistantsComboBox = this.FindControl<ComboBox>("AssistantsComboBox");
        assistantsComboBox.ItemsSource = _db.Employees.ToList();
    }

    private void SetEditMode(bool isEditing)
    {
        _isEditing = isEditing;
        
      
        var lastNameTextBox = this.FindControl<TextBox>("LastNameTextBox");
        var firstNameTextBox = this.FindControl<TextBox>("FirstNameTextBox");
        var middleNameTextBox = this.FindControl<TextBox>("MiddleNameTextBox");
        var mobilePhoneTextBox = this.FindControl<TextBox>("MobilePhoneTextBox");
        var workPhoneTextBox = this.FindControl<TextBox>("WorkPhoneTextBox");
        var emailTextBox = this.FindControl<TextBox>("EmailTextBox");
        var birthDatePicker = this.FindControl<DatePicker>("BirthDatePicker");
        var departmentsComboBox = this.FindControl<ComboBox>("DepartmentsComboBox");
        var positionsComboBox = this.FindControl<ComboBox>("PositionsComboBox");
        var assistantsComboBox = this.FindControl<ComboBox>("AssistantsComboBox");
        
     
        lastNameTextBox.IsReadOnly = !isEditing;
        firstNameTextBox.IsReadOnly = !isEditing;
        middleNameTextBox.IsReadOnly = !isEditing;
        mobilePhoneTextBox.IsReadOnly = !isEditing;
        workPhoneTextBox.IsReadOnly = !isEditing;
        emailTextBox.IsReadOnly = !isEditing;
        birthDatePicker.IsEnabled = isEditing;
        departmentsComboBox.IsEnabled = isEditing;
        positionsComboBox.IsEnabled = isEditing;
        assistantsComboBox.IsEnabled = isEditing;
        OfficeTextBox.IsReadOnly = !isEditing;
        
    
        var editButton = this.FindControl<Button>("EditButton");
        var saveButton = this.FindControl<Button>("SaveButton");
        var cancelButton = this.FindControl<Button>("CancelButton");
        
        editButton.IsVisible = !isEditing;
        saveButton.IsVisible = isEditing;
        cancelButton.IsVisible = isEditing;
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        SetEditMode(true);
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (!Validate())
            return;
        _employee.Lastname = this.FindControl<TextBox>("LastNameTextBox").Text;
        _employee.Firstname = this.FindControl<TextBox>("FirstNameTextBox").Text;
        _employee.Middlename = this.FindControl<TextBox>("MiddleNameTextBox").Text;
        _employee.Mobilephone = this.FindControl<TextBox>("MobilePhoneTextBox").Text;
        _employee.Workphone = this.FindControl<TextBox>("WorkPhoneTextBox").Text;
        _employee.Email = this.FindControl<TextBox>("EmailTextBox").Text;
        _employee.Office = this.FindControl<TextBox>("OfficeTextBox").Text;
        _employee.Birthdate = this.FindControl<DatePicker>("BirthDatePicker").SelectedDate?.Date is { } dt
            ? DateOnly.FromDateTime(dt)
            : null;
        _employee.Subdivisionid = (this.FindControl<ComboBox>("DepartmentsComboBox").SelectedItem as DepartmentSubdivision)?.Subdivisionid ?? 0;
        _employee.Positionid = (this.FindControl<ComboBox>("PositionsComboBox").SelectedItem as Position)?.Positionid ?? 0;

        
        if (_employee.Employeeid == 0)
        {
            _db.Employees.Add(_employee);
            _db.SaveChanges(); 
            _employee.Personalnumber = $"EMP{_employee.Employeeid}";
            _db.Employees.Update(_employee);
        }
        else
        {
            _db.Employees.Update(_employee);
        }

        _db.SaveChanges();
        SetEditMode(false);
    }


    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        LoadEmployee(_employee);
        SetEditMode(false);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private bool Validate()
    {
        var errors = new List<string>();
        var lastName = this.FindControl<TextBox>("LastNameTextBox").Text;
        var firstName = this.FindControl<TextBox>("FirstNameTextBox").Text;
        var workPhone = this.FindControl<TextBox>("WorkPhoneTextBox").Text;
        var email = this.FindControl<TextBox>("EmailTextBox").Text;
        var department = this.FindControl<ComboBox>("DepartmentsComboBox").SelectedItem;
        var position = this.FindControl<ComboBox>("PositionsComboBox").SelectedItem;
        var office = this.FindControl<TextBox>("OfficeTextBox").Text;

        if (string.IsNullOrWhiteSpace(lastName))
            errors.Add("Фамилия обязательна для заполнения");
        if (string.IsNullOrWhiteSpace(firstName))
            errors.Add("Имя обязательно для заполнения");
        if (string.IsNullOrWhiteSpace(workPhone))
            errors.Add("Рабочий телефон обязателен для заполнения");
        if (string.IsNullOrWhiteSpace(email))
            errors.Add("Email обязателен для заполнения");
        if (department == null)
            errors.Add("Структурное подразделение обязательно для заполнения");
        if (position == null)
            errors.Add("Должность обязательна для заполнения");
        if (string.IsNullOrWhiteSpace(office))
        {
            errors.Add("Заполните поле офис");
        }

       
        var phoneRegex = new Regex(@"^[0-9+()\-\s#]{1,20}$");
        var mobilePhone = this.FindControl<TextBox>("MobilePhoneTextBox").Text;
        if (!string.IsNullOrEmpty(mobilePhone) && !phoneRegex.IsMatch(mobilePhone))
            errors.Add("Мобильный телефон содержит недопустимые символы или превышает 20 символов");
        if (!phoneRegex.IsMatch(workPhone))
            errors.Add("Рабочий телефон содержит недопустимые символы или превышает 20 символов");

      
        var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        if (!emailRegex.IsMatch(email))
            errors.Add("Некорректный формат email");

        if (errors.Any())
        {
            var message = string.Join("\n", errors);
            // TODO: Show error message to user
            return false;
        }

        return true;
    }

    private void LoadEmployee(Employee employee)
    {
        this.FindControl<TextBox>("LastNameTextBox").Text = employee.Lastname;
        this.FindControl<TextBox>("FirstNameTextBox").Text = employee.Firstname;
        this.FindControl<TextBox>("MiddleNameTextBox").Text = employee.Middlename;
        this.FindControl<TextBox>("MobilePhoneTextBox").Text = employee.Mobilephone;
        this.FindControl<TextBox>("WorkPhoneTextBox").Text = employee.Workphone;
        this.FindControl<TextBox>("EmailTextBox").Text = employee.Email;
        this.FindControl<DatePicker>("BirthDatePicker").SelectedDate = employee.Birthdate.HasValue
            ? new DateTimeOffset(employee.Birthdate.Value.ToDateTime(TimeOnly.MinValue))
            : null;
        this.FindControl<ComboBox>("DepartmentsComboBox").SelectedItem = employee.Subdivision;
        this.FindControl<ComboBox>("PositionsComboBox").SelectedItem = employee.Position;
        this.FindControl<TextBox>("OfficeTextBox").Text = employee.Office;

    }
}

