USE [McpData]
GO
/****** Object:  Schema [mcp]    Script Date: 06.08.2026 13:41:58 ******/
CREATE SCHEMA [mcp]
GO
/****** Object:  Table [dbo].[Customers]    Script Date: 06.08.2026 13:41:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Customers](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](120) NOT NULL,
	[Email] [varchar](120) NOT NULL,
	[Address] [nvarchar](500) NULL,
 CONSTRAINT [PK_Customers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  UserDefinedFunction [mcp].[Find_Customers_By_Name]    Script Date: 06.08.2026 13:41:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Dai Duong Nguyen
-- Create date: 30.07.2026
-- Description:	Find customers by name
-- =============================================
CREATE   FUNCTION [mcp].[Find_Customers_By_Name]
(	
	@name nvarchar(120)
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT Id, Name, Email, Address
	FROM dbo.Customers
	WHERE Name LIKE '%' + @name + '%'
)
GO
/****** Object:  View [mcp].[All_Customers]    Script Date: 06.08.2026 13:41:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   VIEW [mcp].[All_Customers] AS
SELECT 
	Id,
	Name,
	Email,
	Address
FROM dbo.Customers
GO
/****** Object:  UserDefinedFunction [mcp].[Find_Customers_By_Email]    Script Date: 06.08.2026 13:41:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Dai Duong Nguyen
-- Create date: 30.07.2026
-- Description:	Find customers by name
-- =============================================
CREATE   FUNCTION [mcp].[Find_Customers_By_Email]
(	
	@email nvarchar(120)
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT Id, Name, Email, Address
	FROM dbo.Customers
	WHERE LOWER(Email) LIKE @email
)
GO
/****** Object:  Table [dbo].[OrderItems]    Script Date: 06.08.2026 13:41:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrderItems](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Order_Id] [int] NOT NULL,
	[ItemNumber] [int] NOT NULL,
	[ProductPrice_Id] [int] NOT NULL,
	[ProductName] [nvarchar](120) NULL,
	[ProductPrice] [money] NOT NULL,
	[Amount] [money] NOT NULL,
	[State] [int] NOT NULL,
	[Description] [nvarchar](max) NULL,
 CONSTRAINT [PK_OrderItems] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Orders]    Script Date: 06.08.2026 13:41:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Orders](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Customer_Id] [int] NOT NULL,
	[OrderNumber] [varchar](20) NOT NULL,
	[OrderDate] [date] NOT NULL,
	[OrderState] [int] NOT NULL,
 CONSTRAINT [PK_Orders] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProductCategories]    Script Date: 06.08.2026 13:41:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProductCategories](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[MatchCode] [varchar](20) NOT NULL,
	[Name] [nvarchar](120) NOT NULL,
 CONSTRAINT [PK_ProductCategories] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProductPrices]    Script Date: 06.08.2026 13:41:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProductPrices](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Product_Id] [int] NOT NULL,
	[Variant_Id] [int] NULL,
	[Price] [money] NOT NULL,
	[Description] [nvarchar](max) NULL,
 CONSTRAINT [PK_ProductPrices] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Products]    Script Date: 06.08.2026 13:41:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Products](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ProductCategory_Id] [int] NOT NULL,
	[MatchCode] [varchar](20) NOT NULL,
	[Name] [nvarchar](120) NOT NULL,
	[Description] [nvarchar](max) NULL,
 CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProductVariants]    Script Date: 06.08.2026 13:41:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProductVariants](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Code] [varchar](4) NOT NULL,
	[DisplayName] [nvarchar](120) NULL,
	[Description] [nvarchar](max) NULL,
 CONSTRAINT [PK_ProductVariants] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UX_Customers]    Script Date: 06.08.2026 13:41:58 ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_Customers] ON [dbo].[Customers]
(
	[Email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UX_Orders]    Script Date: 06.08.2026 13:41:58 ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_Orders] ON [dbo].[Orders]
(
	[OrderNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[OrderItems]  WITH CHECK ADD  CONSTRAINT [FK_OrderItems_Orders] FOREIGN KEY([Order_Id])
REFERENCES [dbo].[Orders] ([Id])
GO
ALTER TABLE [dbo].[OrderItems] CHECK CONSTRAINT [FK_OrderItems_Orders]
GO
ALTER TABLE [dbo].[OrderItems]  WITH CHECK ADD  CONSTRAINT [FK_OrderItems_ProductPrices] FOREIGN KEY([ProductPrice_Id])
REFERENCES [dbo].[ProductPrices] ([Id])
GO
ALTER TABLE [dbo].[OrderItems] CHECK CONSTRAINT [FK_OrderItems_ProductPrices]
GO
ALTER TABLE [dbo].[Orders]  WITH CHECK ADD  CONSTRAINT [FK_Orders_Customers] FOREIGN KEY([Customer_Id])
REFERENCES [dbo].[Customers] ([Id])
GO
ALTER TABLE [dbo].[Orders] CHECK CONSTRAINT [FK_Orders_Customers]
GO
ALTER TABLE [dbo].[ProductPrices]  WITH CHECK ADD  CONSTRAINT [FK_ProductPrices_Products] FOREIGN KEY([Product_Id])
REFERENCES [dbo].[Products] ([Id])
GO
ALTER TABLE [dbo].[ProductPrices] CHECK CONSTRAINT [FK_ProductPrices_Products]
GO
ALTER TABLE [dbo].[ProductPrices]  WITH CHECK ADD  CONSTRAINT [FK_ProductPrices_ProductVariants] FOREIGN KEY([Variant_Id])
REFERENCES [dbo].[ProductVariants] ([Id])
GO
ALTER TABLE [dbo].[ProductPrices] CHECK CONSTRAINT [FK_ProductPrices_ProductVariants]
GO
ALTER TABLE [dbo].[Products]  WITH CHECK ADD  CONSTRAINT [FK_Products_ProductCategories] FOREIGN KEY([ProductCategory_Id])
REFERENCES [dbo].[ProductCategories] ([Id])
GO
ALTER TABLE [dbo].[Products] CHECK CONSTRAINT [FK_Products_ProductCategories]
GO
/****** Object:  StoredProcedure [mcp].[Create_Customer]    Script Date: 06.08.2026 13:41:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Dai Duong Nguyen
-- Create date: 30.07.2026
-- Description:	Create or update a customer
-- =============================================
CREATE   PROCEDURE [mcp].[Create_Customer] 
	@Name nvarchar(120), 
	@Email varchar(120),
	@Address nvarchar(500)
AS
BEGIN
	SET NOCOUNT ON;
	SET @Email = LOWER(@Email)
	IF EXISTS(SELECT 1 FROM dbo.Customers WHERE @Email IS NOT NULL AND LOWER(Email) = @Email)
		UPDATE dbo.Customers
		SET Name = @Name, Address = @Address
		WHERE LOWER(Email) = @Email
	ELSE
	    INSERT INTO dbo.Customers(Name, Email, Address)
		VALUES (@Name, LOWER(@Email), @Address)
	SELECT Id FROM dbo.Customers WHERE LOWER(Email) = @Email
END
GO
/****** Object:  StoredProcedure [mcp].[Update_Customer]    Script Date: 06.08.2026 13:41:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Dai Duong Nguyen
-- Create date: 30.07.2026
-- Description:	Create or update a customer
-- =============================================
CREATE    PROCEDURE [mcp].[Update_Customer] 
	@Id INT,
	@Name nvarchar(120), 
	@Email varchar(120),
	@Address nvarchar(500)
AS
BEGIN
	SET NOCOUNT ON;
	SET @Email = LOWER(@Email)
	IF EXISTS(SELECT 1 FROM dbo.Customers WHERE ISNULL(@Id, 0) = 0 AND @Email IS NOT NULL AND LOWER(Email) = @Email)
		UPDATE dbo.Customers
		SET Name = ISNULL(@Name, Name), Address = ISNULL(@Address, Address)
		WHERE LOWER(Email) = @Email
	ELSE
		UPDATE dbo.Customers
		SET Name = ISNULL(@Name, Name), Email = ISNULL(LOWER(@Email), Email), Address = ISNULL(@Address, Address)
		WHERE Id = @Id
	SELECT TOP 1 Id FROM dbo.Customers WHERE Id=@Id OR (ISNULL(@Id, 0) = 0 AND @Email IS NOT NULL AND LOWER(Email) = @Email) ORDER BY Id
END
GO
EXEC [McpData].sys.sp_addextendedproperty @name=N'Mcp_Title', @value=N'%APPNAME% %INSTANCE%' 
GO
EXEC [McpData].sys.sp_addextendedproperty @name=N'Mcp_Description', @value=N'Example database [%DBNAME%] on [%SERVER%] for testing MCP server %APPNAME% version %APPVERSION%. 
All mcp tools are stored as views, functions or procedures in the schema %SCHEMA%' 
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Erstelle einen neuen Kunden. Wenn die Email-Adresse gefunden wurde, wird kein neuer Datensatz erstellt sondern der gefundene Kunde aktualisiert' , @level0type=N'SCHEMA',@level0name=N'mcp', @level1type=N'PROCEDURE',@level1name=N'Create_Customer'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Aktualisiere einen Kunden beim Finden von Kunden-ID oder Email-Adresse ohne Kunden-ID' , @level0type=N'SCHEMA',@level0name=N'mcp', @level1type=N'PROCEDURE',@level1name=N'Update_Customer'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Sucht Kunden nach Emailadresse' , @level0type=N'SCHEMA',@level0name=N'mcp', @level1type=N'FUNCTION',@level1name=N'Find_Customers_By_Email'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Sucht Kunden nach Namen' , @level0type=N'SCHEMA',@level0name=N'mcp', @level1type=N'FUNCTION',@level1name=N'Find_Customers_By_Name'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Liste der vorhandenen Kunden' , @level0type=N'SCHEMA',@level0name=N'mcp', @level1type=N'VIEW',@level1name=N'All_Customers'
GO
USE [master]
GO
ALTER DATABASE [McpData] SET  READ_WRITE 
GO
