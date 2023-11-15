// MIT License
// 
// Copyright (c) 2023 Russell Camo (Russkyc)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Core.Entities;
using Infrastructure.Data;
using MongoFramework.Linq;
using SimpleCrud.Events;
using SimpleCrud.Extensions;
using SimpleCrud.Models;
using SimpleCrud.Services;

namespace SimpleCrud.ViewModels;

[SuppressMessage("ReSharper", "AsyncVoidLambda")]
public partial class StudentManagementViewModel : ObservableObject
{
    private readonly DbContext _dbContext;
    private readonly DialogService _dialogService;

    [ObservableProperty]
    private Student? _activeStudent;

    [ObservableProperty]
    private ObservableCollection<ObservableStudent> _students = new();

    [ObservableProperty]
    private ObservableCollection<Course> _courses = new();

    public StudentManagementViewModel(DbContext dbContext, DialogService dialogService)
    {
        _dbContext = dbContext;
        _dialogService = dialogService;
        
        RegisterObserver();
        InitializeCollections().SafeFireAndForget();
    }

    void RegisterObserver()
    {
        WeakReferenceMessenger.Default.Register<EditStudentEvent>(this,
                async (_, args) => await OnEditStudent(args));
        WeakReferenceMessenger.Default.Register<RemoveStudentEvent>(this,
                async (_, args) => await OnRemoveStudent(args));
    }

    async Task InitializeCollections()
    {
        var students = await _dbContext.Students.ToListAsync();
        Students = students.Select(student => student.ToObservableStudent())
            .ToObservableCollection();
        
        var courses = await _dbContext.Courses.ToListAsync();
        Courses = courses.ToObservableCollection();
    }

    [RelayCommand]
    async Task OpenStudentCreateDialog()
    {
        ActiveStudent ??= new();

        if (!await _dialogService.OpenStudentFormDialog(this, "Add Student"))
        {
            return;
        }

        var validation = ActiveStudent.AreStudentDetailsValid();

        if (!validation.Result)
        {
            await _dialogService.OpenNotifyDialog("Student Management", validation.Message);
            OpenStudentCreateDialog().SafeFireAndForget();
            return;
        }

        if (!await _dialogService.OpenPromptDialog("Student Management",
                $"Add {ActiveStudent.FirstName} to Students?"))
        {
            ActiveStudent = null;
            return;
        }
        
        _dbContext.Students.Add(ActiveStudent);
        await _dbContext.SaveChangesAsync()
            .ContinueWith(_ => Students.Add(ActiveStudent.ToObservableStudent()));
        ActiveStudent = null;
    }
    
    private async Task OnEditStudent(EditStudentEvent message)
    {
        ActiveStudent = message.Value;

        if (!await _dialogService.OpenStudentFormDialog(this, $"Edit {ActiveStudent.FirstName}"))
        {
            ActiveStudent = null;
            return;
        }

        if (!await _dialogService.OpenPromptDialog("Student Management", "Save changes?"))
        {
            return;
        }
        
        _dbContext.Students.Update(ActiveStudent);
        _dbContext.SaveChangesAsync()
            .ContinueWith(_ => InitializeCollections())
            .SafeFireAndForget();
        ActiveStudent = null;
    }

    private async Task OnRemoveStudent(RemoveStudentEvent message)
    {
        if (!await _dialogService.OpenPromptDialog("Student Management",
                $"Are you sure you want to remove {message.Value.FirstName} from the students list?"))
        {
            return;
        }
        
        _dbContext.Students.Remove(message.Value);
        _dbContext.SaveChangesAsync()
            .ContinueWith(_ => InitializeCollections())
            .SafeFireAndForget();
    }

    
}