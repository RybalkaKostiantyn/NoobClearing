DECLARE @ResourceID INT
		,@ResourcesCultureID INT		
		,@LocalizedText_en_US nvarchar(4000)
		,@LocalizedText_fr_FR nvarchar(4000)
		,@LocalizedText_de_DE nvarchar(4000)
		,@LocalizedText_ja_JP nvarchar(4000)
		,@LocalizedText_es_ES nvarchar(4000)
		,@LocalizedText_nl_NL nvarchar(4000)
		,@LocalizedText_pt_BR nvarchar(4000)
		,@LocalizedText_en_GB nvarchar(4000)
		,@Project varchar(150)
		,@Page varchar(150)
		,@Control varchar(150)
		,@Property varchar(150)
		,@StringID varchar(150)
		,@ApplicationTypeID int
		,@NoLocalization varchar(150)

SET @ApplicationTypeID = '{0}'; --admin=2 customer=1
SET @StringID = '{1}';
SET @Project =  NULL;
SET @Page = NULL;
SET @Control = NULL;
SET @Property = NULL;
SET @LocalizedText_en_US = '{2}';
SET @LocalizedText_fr_FR = '{3}';
SET @LocalizedText_de_DE = '{4}';
SET @LocalizedText_ja_JP = '{5}';
SET @LocalizedText_es_ES = '{6}';
SET @LocalizedText_nl_NL = '{7}';
SET @LocalizedText_pt_BR = '{8}';
SET @LocalizedText_en_GB = '{9}';
SET @NoLocalization = '%no%localization%'


IF NOT EXISTS (SELECT * FROM [dbo].[Resource] WHERE ApplicationTypeID = '{0}' AND  StringID = '{1}')
BEGIN
	-- looking for the first free ResourceID in the Resource table
	SELECT @ResourceID = MIN(ResourceID + 1) FROM [dbo].[Resource] t1
	WHERE NOT EXISTS (SELECT * FROM [dbo].[Resource] t2 WHERE t1.ResourceID + 1 = t2.ResourceID)
		AND ResourceID >= 6000 AND ResourceID < 7000
	
	IF @ResourceID IS NULL BEGIN
	SET 
		@ResourceID = 6000;
	END
	
	SET @ResourcesCultureID = @ResourceID;
	
	-- Add rows to [dbo].[Resource]
	SET IDENTITY_INSERT [dbo].[Resource] ON
	INSERT INTO [dbo].[Resource] 
	([ResourceID], 
	[Project], 
	[Page], 
	[Control], 
	[Property], 
	[StringID],
	[ApplicationTypeID]) 
	VALUES (@ResourceID, @Project, @Page, @Control, @Property, @StringID, @ApplicationTypeID)
	SET IDENTITY_INSERT [dbo].[Resource] OFF
	
	-- English
	SET IDENTITY_INSERT [dbo].[ResourcesCulture] ON
	INSERT INTO [dbo].[ResourcesCulture] 
	([ResourcesCultureID], 
	[ResourceID], 
	[CultureID], 
	[LocalizedText]) 
	VALUES (@ResourcesCultureID, @ResourceID, 1, @LocalizedText_en_US)
	SET IDENTITY_INSERT [dbo].[ResourcesCulture] OFF
	
	-- French
	SET @ResourcesCultureID = @ResourcesCultureID + 10000;
	SET IDENTITY_INSERT [dbo].[ResourcesCulture] ON
	INSERT INTO [dbo].[ResourcesCulture] 
	([ResourcesCultureID], 
	[ResourceID], 
	[CultureID], 
	[LocalizedText]) 
	VALUES (@ResourcesCultureID, @ResourceID, 2, @LocalizedText_fr_FR)
	SET IDENTITY_INSERT [dbo].[ResourcesCulture] OFF

	-- German
	SET @ResourcesCultureID = @ResourcesCultureID + 10000;
	SET IDENTITY_INSERT [dbo].[ResourcesCulture] ON
	INSERT INTO [dbo].[ResourcesCulture] 
	([ResourcesCultureID], 
	[ResourceID], 
	[CultureID], 
	[LocalizedText]) 
	VALUES (@ResourcesCultureID, @ResourceID, 3, @LocalizedText_de_DE)
	SET IDENTITY_INSERT [dbo].[ResourcesCulture] OFF

	-- Japanese
	SET @ResourcesCultureID = @ResourcesCultureID + 10000;
	SET IDENTITY_INSERT [dbo].[ResourcesCulture] ON
	INSERT INTO [dbo].[ResourcesCulture] 
	([ResourcesCultureID], 
	[ResourceID], 
	[CultureID], 
	[LocalizedText]) 
	VALUES (@ResourcesCultureID, @ResourceID, 4, @LocalizedText_ja_JP)
	SET IDENTITY_INSERT [dbo].[ResourcesCulture] OFF

	-- Spanish
	SET @ResourcesCultureID = @ResourcesCultureID + 10000;
	SET IDENTITY_INSERT [dbo].[ResourcesCulture] ON
	INSERT INTO [dbo].[ResourcesCulture] 
	([ResourcesCultureID], 
	[ResourceID], 
	[CultureID], 
	[LocalizedText]) 
	VALUES (@ResourcesCultureID, @ResourceID, 5, @LocalizedText_es_ES)
	SET IDENTITY_INSERT [dbo].[ResourcesCulture] OFF

	-- Dutch
	SET @ResourcesCultureID = @ResourcesCultureID + 10000;
	SET IDENTITY_INSERT [dbo].[ResourcesCulture] ON
	INSERT INTO [dbo].[ResourcesCulture] 
	([ResourcesCultureID], 
	[ResourceID], 
	[CultureID], 
	[LocalizedText]) 
	VALUES (@ResourcesCultureID, @ResourceID, 6, @LocalizedText_nl_NL)
	SET IDENTITY_INSERT [dbo].[ResourcesCulture] OFF

	-- Portuguese
	SET @ResourcesCultureID = @ResourcesCultureID + 10000;
	SET IDENTITY_INSERT [dbo].[ResourcesCulture] ON
	INSERT INTO [dbo].[ResourcesCulture] 
      ([ResourcesCultureID], 
      [ResourceID], 
      [CultureID], 
      [LocalizedText]) 
	VALUES (@ResourcesCultureID, @ResourceID, 7, @LocalizedText_pt_BR)
	SET IDENTITY_INSERT [dbo].[ResourcesCulture] OFF

	-- English - United Kingdom
	SET @ResourcesCultureID = @ResourcesCultureID + 10000;
	SET IDENTITY_INSERT [dbo].[ResourcesCulture] ON
	INSERT INTO [dbo].[ResourcesCulture] 
      ([ResourcesCultureID], 
      [ResourceID], 
      [CultureID], 
      [LocalizedText]) 
	VALUES (@ResourcesCultureID, @ResourceID, 8, @LocalizedText_en_GB)
	SET IDENTITY_INSERT [dbo].[ResourcesCulture] OFF
END
ELSE
BEGIN
	SELECT @ResourceID = ResourceID
	FROM [dbo].[Resource] 
	WHERE ApplicationTypeID = '{0}' AND  StringID = '{1}'

	-- English
	IF @LocalizedText_en_US NOT LIKE @NoLocalization
	BEGIN
		UPDATE [dbo].[ResourcesCulture]
		SET [LocalizedText] = @LocalizedText_en_US
		WHERE [ResourceID] = @ResourceID AND [CultureID] = 1;
	END

	-- French
	IF @LocalizedText_fr_FR NOT LIKE @NoLocalization
	BEGIN
		UPDATE [dbo].[ResourcesCulture]
		SET [LocalizedText] = @LocalizedText_fr_FR
		WHERE [ResourceID] = @ResourceID AND [CultureID] = 2;
	END

	-- Spanish
	IF @LocalizedText_de_DE NOT LIKE @NoLocalization
	BEGIN
		UPDATE [dbo].[ResourcesCulture]
		SET [LocalizedText] = @LocalizedText_de_DE
		WHERE [ResourceID] = @ResourceID AND [CultureID] = 3;
	END

	-- Japanese
	IF @LocalizedText_ja_JP NOT LIKE @NoLocalization
	BEGIN
		UPDATE [dbo].[ResourcesCulture]
		SET [LocalizedText] = @LocalizedText_ja_JP
		WHERE [ResourceID] = @ResourceID AND [CultureID] = 4;
	END

	-- Japanese
	IF @LocalizedText_es_ES NOT LIKE @NoLocalization
	BEGIN
		UPDATE [dbo].[ResourcesCulture]
		SET [LocalizedText] = @LocalizedText_es_ES
		WHERE [ResourceID] = @ResourceID AND [CultureID] = 5;
	END

	-- Dutch
	IF @LocalizedText_nl_NL NOT LIKE @NoLocalization
	BEGIN
		UPDATE [dbo].[ResourcesCulture]
		SET [LocalizedText] = @LocalizedText_nl_NL
		WHERE [ResourceID] = @ResourceID AND [CultureID] = 6;
	END

	-- Portuguese
	IF @LocalizedText_pt_BR NOT LIKE @NoLocalization
	BEGIN
		UPDATE [dbo].[ResourcesCulture]
		SET [LocalizedText] = @LocalizedText_pt_BR
		WHERE [ResourceID] = @ResourceID AND [CultureID] = 7;
	END

	-- English - United Kingdom
	IF @LocalizedText_en_GB NOT LIKE @NoLocalization
	BEGIN
		UPDATE [dbo].[ResourcesCulture]
		SET [LocalizedText] = @LocalizedText_en_GB
		WHERE [ResourceID] = @ResourceID AND [CultureID] = 8;
	END
END
