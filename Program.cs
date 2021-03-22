using pdfmerger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


var creators = new List<(string dirName, IPdfMerger merger)>
{
    new ("itextpdf", new ITextPdfCreator()),
    new ("itextosspdf", new ITextPdfOSSCreator())
};


foreach (var creatorSetup in creators)
{
    foreach (var dataObj in GetData(creatorSetup.dirName))
    {
        var file = await creatorSetup.merger.CreateAsync(dataObj.FilesToMerge.Select(x => (x.ContentType, x.Content())));

        using var fileStream = new FileStream($"../../../results/{creatorSetup.dirName}/{dataObj.ExpectedFileName}", FileMode.Create, FileAccess.ReadWrite);
        fileStream.Write(file, 0, file.Length);
        fileStream.Flush();
    }
}

static IEnumerable<Data> GetData(string dirName)
{
    return new List<Data>
    {
        new()
        {
            FilesToMerge = new()
            {
                new("image/jpg", () => File.ReadAllBytes($"../../../resources/timesheet-image.jpg")),
                new("image/png", () => File.ReadAllBytes($"../../../resources/timesheet-image2.png")),
            },
            ExpectedFileName = "merged-images.pdf"
        },
        new()
        {
            FilesToMerge = new()
            {
                new("application/pdf", () => File.ReadAllBytes($"../../../resources/timesheet-pdf.pdf")),
                new("application/pdf", () => File.ReadAllBytes($"../../../resources/timesheet-2-pdf.pdf")),
            },
            ExpectedFileName = "merged-pdfs.pdf"
        },
        new()
        {
            FilesToMerge = new()
            {
                new("application/pdf", () => File.ReadAllBytes($"../../../results/{dirName}/merged-images.pdf")),
                new("application/pdf", () => File.ReadAllBytes($"../../../results/{dirName}/merged-pdfs.pdf")),
            },
            ExpectedFileName = "merged-merged.pdf"
        },
        new()
        {
            FilesToMerge = new()
            {
                new("image/png", () => File.ReadAllBytes($"../../../resources/timesheet-image2.png")),
                new("application/pdf", () => File.ReadAllBytes($"../../../results/{dirName}/merged-merged.pdf")),

            },
            ExpectedFileName = "image2-merged-merged-merged.pdf",
        },
        new()
        {
            FilesToMerge = new()
            {
                new("application/pdf", () => File.ReadAllBytes($"../../../results/{dirName}/merged-merged.pdf")),
                new("image/png", () => File.ReadAllBytes($"../../../resources/timesheet-image2.png")),
            },
            ExpectedFileName = "merged-merged-image2-merged.pdf",
        }
    };
}

class Data
{
    public List<(string ContentType, Func<byte[]> Content)> FilesToMerge { get; set; }
    public string ExpectedFileName { get; set; }
}
