using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Testify.Shared.DTOs.KanbanTasks;
using Testify.Shared.DTOs.TaskActivity;

namespace Testify.Services
{
    public class AuditTrailPdfDocument : IDocument
    {
        private readonly KanbanTaskResponse _task;
        private readonly IEnumerable<TaskActivityResponse> _activities;

        public AuditTrailPdfDocument(KanbanTaskResponse task, IEnumerable<TaskActivityResponse> activities)
        {
            _task = task;
            _activities = activities;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(header =>
                {
                    header.Item().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("AUDIT TRAIL")
                                .FontSize(8).Bold().FontColor(Colors.Grey.Medium);
                            col.Item().Text(_task.Title)
                                .FontSize(20).Bold().FontColor(Colors.Grey.Darken4);
                            col.Item().Text($"TASK-{_task.Id}")
                                .FontSize(8).FontColor(Colors.Grey.Medium);
                        });
                        row.ConstantItem(100).AlignRight().Column(col =>
                        {
                            col.Item().Text($"Generated")
                                .FontSize(8).FontColor(Colors.Grey.Medium);
                            col.Item().Text(DateTime.Now.ToString("MMM dd, yyyy"))
                                .FontSize(8).Bold();
                        });
                    });
                    header.Item().PaddingTop(8).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(0);
                });

                page.Content().PaddingTop(20).Column(content =>
                {
                    foreach (var log in _activities)
                    {
                        content.Item()
                            .BorderBottom(1).BorderColor(Colors.Grey.Lighten3)
                            .PaddingVertical(10)
                            .Row(row =>
                            {
                                row.ConstantItem(16).AlignMiddle().Column(marker =>
                                {
                                    marker.Item().Width(8).Height(8)
                                        .Border(0)
                                        .CornerRadius(4)
                                        .Background(GetActionColor(log.Action));
                                });

                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Row(r =>
                                    {
                                        r.RelativeItem().Text(log.Action).Bold().FontSize(11);
                                        r.ConstantItem(160).AlignRight()
                                            .Text(GetLogTypeLabel(log.Action))
                                            .FontSize(8).FontColor(Colors.Grey.Medium);
                                    });

                                    col.Item().PaddingTop(2).Row(r =>
                                    {
                                        r.RelativeItem()
                                            .Text($"{log.FullName}  •  {log.CreatedAt:MMM dd, yyyy  HH:mm}")
                                            .FontSize(8).FontColor(Colors.Grey.Medium);
                                    });

                                    if (!string.IsNullOrEmpty(log.OldValue) && !string.IsNullOrEmpty(log.NewValue))
                                    {
                                        col.Item().PaddingTop(6).Row(r =>
                                        {
                                            r.AutoItem().Background(Colors.Grey.Lighten3)
                                                .Padding(4).Text(log.OldValue)
                                                .FontSize(8).FontColor(Colors.Grey.Darken1);
                                            r.AutoItem().AlignMiddle().PaddingHorizontal(6)
                                                .Text("→").FontColor(Colors.Grey.Medium);
                                            r.AutoItem().Background(Colors.Blue.Lighten4)
                                                .Padding(4).Text(log.NewValue)
                                                .FontSize(8).Bold().FontColor(Colors.Blue.Darken2);
                                        });
                                    }
                                    else if (!string.IsNullOrEmpty(log.Description))
                                    {
                                        col.Item().PaddingTop(6)
                                            .Text($"\"{log.Description}\"")
                                            .FontSize(9).Italic().FontColor(Colors.Grey.Darken1);
                                    }
                                });
                            });
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ").FontSize(8).FontColor(Colors.Grey.Medium);
                    x.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                    x.Span(" of ").FontSize(8).FontColor(Colors.Grey.Medium);
                    x.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
                });
            });
        }

        private static string GetActionColor(string action) => action switch
        {
            "Created" or "Status Changed" => Colors.Blue.Medium,
            "Assignee Changed" => Colors.Purple.Medium,
            "Updated" or "Priority Changed" or "Type Changed"
                or "Milestone Changed" or "Due Date Changed" => Colors.Orange.Medium,
            _ => Colors.Grey.Medium
        };

        private static string GetLogTypeLabel(string action) => action switch
        {
            "Created" or "Status Changed" => "STATE",
            "Assignee Changed" => "PERSONNEL",
            "Updated" or "Priority Changed" or "Type Changed"
                or "Milestone Changed" or "Due Date Changed" => "METADATA",
            _ => "EVENT"
        };
    }
}