using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using Stylet;
using TextureMapBuilder.Models;

namespace TextureMapBuilder.Pages;

public class ShellViewModel : Screen
{
    private OpenFileDialog openFileDialog;

    public List<FileEntry> FileList { get; set; }

    public List<ComboOption> SortOptions { get; set; }

    public string Sort { get; set; } = "";

    public int Rows { get; set; } = 0;

    public int Cols { get; set; } = 1;

    public int Skips { get; set; } = 0;

    public bool UseLastFrame { get; set; } = true;

    public byte[] GeneratedImage { get; set; }

    public string Information { get; set; } = "";

    public ShellViewModel()
    {
        var sortOptions = new List<ComboOption>();
        sortOptions.Add(new ComboOption("(默认)", ""));
        sortOptions.Add(new ComboOption("文件名", "filename"));
        this.SortOptions = sortOptions;

        this.openFileDialog = new OpenFileDialog()
        {
            Filter = "图像文件|*.png;*.jpg;*.jpeg;*.bmp;*.webp",
            Multiselect = true
        };
        this.FileList = new List<FileEntry>();
    }

    public void OpenFiles()
    {
        var fileList = new List<string>();
        if (this.openFileDialog.ShowDialog() == true)
        {
            fileList.AddRange(this.openFileDialog.FileNames);
        }

        // Sort
        if (this.Sort == "filename")
        {
            fileList.Sort();
        }

        this.FileList = fileList
            .Select(
                x => new FileEntry
                {
                    FullPath = x,
                    DisplayName = new FileInfo(x).Name
                })
            .ToList();
    }

    public void Preview()
    {
        try
        {
            var list = ImageProcessor.BuildList(this.FileList.Select(x=>x.FullPath), this.Skips, this.UseLastFrame);
            var (payload, info) = ImageProcessor.Merge(list, this.Rows, this.Cols);
            this.GeneratedImage = payload;
            this.Information = $"行：{info.Rows} 列：{info.Cols} 区块：{info.SingleWidth} x {info.SingleHeight}\n尺寸：{info.Width} x {info.Height}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"发生错误：\n{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public bool CanPreview => this.CanGenerate;

    public void Generate()
    {

    }

    public bool CanGenerate => this.Rows > 0 || this.Cols > 0;


    private void SortList()
    {
        if (this.Sort == "filename")
        {
            var fileList = this.FileList.OrderBy(x=>x.DisplayName);
            this.FileList = fileList.ToList();
        }
    }

    protected override void OnPropertyChanged(string propertyName)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == nameof(this.Sort))
        {
            this.SortList();
        }
    }
}

