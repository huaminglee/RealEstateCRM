
/****** Object:  Table [dbo].[Performances]    Script Date: 01/01/2014 20:11:57 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Performances](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ProjectId] [int] NOT NULL,
	[RoomType] [nvarchar](max) NULL,
	[BeginDate] [datetime] NOT NULL,
	[CallInNum] [int] NOT NULL,
	[CallVisitNum] [int] NOT NULL,
	[VisitNum] [int] NOT NULL,
	[CardNum] [int] NOT NULL,
	[OrderNum] [int] NOT NULL,
	[Remark] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.Performances] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

insert Functions values(4,'-','统计报表')
insert Functions values(1,'统计报表','项目指标设置')
insert Functions values(1,'统计报表','公司报表')
insert Functions values(1,'统计报表','项目报表')
