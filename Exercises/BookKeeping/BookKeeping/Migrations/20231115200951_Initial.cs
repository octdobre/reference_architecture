using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BookKeeping.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ad",
                columns: table => new
                {
                    Identity = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "varchar(1024)", unicode: false, maxLength: 1024, nullable: false),
                    PublishedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Content = table.Column<string>(type: "varchar(1024)", unicode: false, maxLength: 1024, nullable: false),
                    Advertiser = table.Column<string>(type: "varchar(1024)", unicode: false, maxLength: 1024, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ad", x => x.Identity);
                });

            migrationBuilder.CreateTable(
                name: "Authors",
                columns: table => new
                {
                    Identity = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    LastName = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    YearOfBirth = table.Column<long>(type: "bigint", nullable: true),
                    Nationality = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Authors", x => x.Identity);
                });

            migrationBuilder.CreateTable(
                name: "Cabinets",
                columns: table => new
                {
                    Identity = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Category = table.Column<string>(type: "varchar(1024)", unicode: false, maxLength: 1024, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cabinets", x => x.Identity);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Identity = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "varchar(1024)", unicode: false, maxLength: 1024, nullable: true),
                    ContactAndPayment = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Identity);
                });

            migrationBuilder.CreateTable(
                name: "JobOffer",
                columns: table => new
                {
                    Identity = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "varchar(1024)", unicode: false, maxLength: 1024, nullable: false),
                    PublishedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Content = table.Column<string>(type: "varchar(1024)", unicode: false, maxLength: 1024, nullable: false),
                    Company = table.Column<string>(type: "varchar(1024)", unicode: false, maxLength: 1024, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobOffer", x => x.Identity);
                });

            migrationBuilder.CreateTable(
                name: "NewsArticle",
                columns: table => new
                {
                    Identity = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "varchar(1024)", unicode: false, maxLength: 1024, nullable: false),
                    PublishedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Content = table.Column<string>(type: "varchar(1024)", unicode: false, maxLength: 1024, nullable: false),
                    Source = table.Column<string>(type: "varchar(1024)", unicode: false, maxLength: 1024, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsArticle", x => x.Identity);
                });

            migrationBuilder.CreateTable(
                name: "Publishers",
                columns: table => new
                {
                    Identity = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Publishers", x => x.Identity);
                });

            migrationBuilder.CreateTable(
                name: "Readers",
                columns: table => new
                {
                    Identity = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "varchar(1024)", unicode: false, maxLength: 1024, nullable: false),
                    LastName = table.Column<string>(type: "varchar(1024)", unicode: false, maxLength: 1024, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Readers", x => x.Identity);
                });

            migrationBuilder.CreateTable(
                name: "Writings",
                columns: table => new
                {
                    Identity = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "varchar(1024)", unicode: false, maxLength: 1024, nullable: false),
                    PublishedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Content = table.Column<string>(type: "varchar(1024)", unicode: false, maxLength: 1024, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Writings", x => x.Identity);
                });

            migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    Identity = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    City = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    Street = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    Number = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    AuthorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.Identity);
                    table.ForeignKey(
                        name: "FK_Addresses_Authors_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Authors",
                        principalColumn: "Identity");
                });

            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    Identity = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "varchar(250)", unicode: false, maxLength: 250, nullable: false),
                    Afterword = table.Column<string>(type: "varchar(1000)", unicode: false, maxLength: 1000, nullable: true, defaultValue: "No content."),
                    AuthorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.Identity);
                    table.ForeignKey(
                        name: "FK_Books_Authors_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Authors",
                        principalColumn: "Identity",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Shelves",
                columns: table => new
                {
                    CabinetIdentity = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shelves", x => new { x.CabinetIdentity, x.Id });
                    table.ForeignKey(
                        name: "FK_Shelves_Cabinets_CabinetIdentity",
                        column: x => x.CabinetIdentity,
                        principalTable: "Cabinets",
                        principalColumn: "Identity",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuthorPublisher",
                columns: table => new
                {
                    AuthorsIdentity = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PublishersIdentity = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorPublisher", x => new { x.AuthorsIdentity, x.PublishersIdentity });
                    table.ForeignKey(
                        name: "FK_AuthorPublisher_Authors_AuthorsIdentity",
                        column: x => x.AuthorsIdentity,
                        principalTable: "Authors",
                        principalColumn: "Identity",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuthorPublisher_Publishers_PublishersIdentity",
                        column: x => x.PublishersIdentity,
                        principalTable: "Publishers",
                        principalColumn: "Identity",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Editors",
                columns: table => new
                {
                    Identity = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    LastName = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    PublisherId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Editors", x => x.Identity);
                    table.ForeignKey(
                        name: "FK_Editors_Publishers_PublisherId",
                        column: x => x.PublisherId,
                        principalTable: "Publishers",
                        principalColumn: "Identity");
                });

            migrationBuilder.CreateTable(
                name: "BookContents",
                columns: table => new
                {
                    Identity = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Content = table.Column<string>(type: "varchar(1024)", unicode: false, maxLength: 1024, nullable: false),
                    BookId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookContents", x => x.Identity);
                    table.ForeignKey(
                        name: "FK_BookContents_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Identity",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Loans",
                columns: table => new
                {
                    Identity = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "LoansHistory")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart"),
                    BookId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "LoansHistory")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart"),
                    ReaderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "LoansHistory")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart"),
                    PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "LoansHistory")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart"),
                    PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:IsTemporal", true)
                        .Annotation("SqlServer:TemporalHistoryTableName", "LoansHistory")
                        .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                        .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                        .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Loans", x => x.Identity);
                    table.ForeignKey(
                        name: "FK_Loans_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Identity",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Loans_Readers_ReaderId",
                        column: x => x.ReaderId,
                        principalTable: "Readers",
                        principalColumn: "Identity",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "LoansHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.CreateTable(
                name: "Reservations",
                columns: table => new
                {
                    Identity = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReaderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.Identity);
                    table.ForeignKey(
                        name: "FK_Reservations_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Identity",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reservations_Readers_ReaderId",
                        column: x => x.ReaderId,
                        principalTable: "Readers",
                        principalColumn: "Identity",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Editors",
                columns: new[] { "Identity", "FirstName", "LastName", "PublisherId" },
                values: new object[] { new Guid("ad57b818-2f0c-4fe2-815a-29f01846fe79"), "John", "Doe", null });

            migrationBuilder.InsertData(
                table: "Publishers",
                columns: new[] { "Identity", "Name" },
                values: new object[,]
                {
                    { new Guid("08cd5199-d8f0-4e17-b32e-04e2884e3720"), "Publisher Works" },
                    { new Guid("a4320bae-69d1-4270-ac77-a497fe4395f4"), "Math works" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_AuthorId",
                table: "Addresses",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_Identity_City_Street_Number",
                table: "Addresses",
                columns: new[] { "Identity", "City", "Street", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuthorPublisher_PublishersIdentity",
                table: "AuthorPublisher",
                column: "PublishersIdentity");

            migrationBuilder.CreateIndex(
                name: "IX_BookContents_BookId",
                table: "BookContents",
                column: "BookId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Books_AuthorId",
                table: "Books",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Books_Title",
                table: "Books",
                column: "Title",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Editors_PublisherId",
                table: "Editors",
                column: "PublisherId",
                unique: true,
                filter: "[PublisherId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Loans_BookId",
                table: "Loans",
                column: "BookId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Loans_ReaderId",
                table: "Loans",
                column: "ReaderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_BookId",
                table: "Reservations",
                column: "BookId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ReaderId",
                table: "Reservations",
                column: "ReaderId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ad");

            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropTable(
                name: "AuthorPublisher");

            migrationBuilder.DropTable(
                name: "BookContents");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "Editors");

            migrationBuilder.DropTable(
                name: "JobOffer");

            migrationBuilder.DropTable(
                name: "Loans")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "LoansHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.DropTable(
                name: "NewsArticle");

            migrationBuilder.DropTable(
                name: "Reservations");

            migrationBuilder.DropTable(
                name: "Shelves");

            migrationBuilder.DropTable(
                name: "Writings");

            migrationBuilder.DropTable(
                name: "Publishers");

            migrationBuilder.DropTable(
                name: "Books");

            migrationBuilder.DropTable(
                name: "Readers");

            migrationBuilder.DropTable(
                name: "Cabinets");

            migrationBuilder.DropTable(
                name: "Authors");
        }
    }
}
