SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[DVHAnalysis_DataEnterInstance](
	[Instance_ID] [int] IDENTITY(1,1) NOT NULL,
	[MRN] [nvarchar](50) NOT NULL,
	[IsReIrradiationEvaluation] [bit] NULL,
	[db_entry_time] [datetime] NOT NULL,
	[db_entry_userid] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_DVHAnalysis_DataEnterInstance] PRIMARY KEY CLUSTERED 
(
	[Instance_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO



CREATE TABLE [dbo].[DVHAnalysis_PlanSetup](
	[PlanSetup_ID] [int] IDENTITY(1,1) NOT NULL,
	[Instance_ID] [int] NOT NULL,
	[PlanSetupID] [nvarchar](50) NOT NULL,
	[MRN] [nvarchar](50) NOT NULL,
	[ARIA_PatientSer] [int] NOT NULL,
	[ARIA_CourseSer] [int] NOT NULL,
	[CourseID] [nvarchar](50) NOT NULL,
	[ARIA_PlanSetupSer] [int] NULL,
	[PlanSum_ID] [int] NULL,
	[PlanSumID] [nvarchar](50) NULL,
	[Component_Weight] [float] NOT NULL,
	[Component_NFractionsPlanned] [int] NULL,
	[Component_NFractionsDelivered] [int] NULL,
	[IsVolumetricDoseComponent] [bit] NULL,
	[IsPaperChartBasedComponent] [bit] NULL,
	[CreationDateTime] [datetime] NULL,
	[db_entry_time] [datetime] NOT NULL,
	[db_entry_userid] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_DVHAnalysis_PlanSetup] PRIMARY KEY CLUSTERED 
(
	[PlanSetup_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


CREATE TABLE [dbo].[DVHAnalysis_PlanSum](
	[PlanSum_ID] [int] IDENTITY(1,1) NOT NULL,
	[Instance_ID] [int] NOT NULL,
	[PlanSumID] [nvarchar](50) NOT NULL,
	[CompomentPlanSetupIDs] [nvarchar](250) NULL,
	[ARIA_PlanSumSer] [int] NULL,
	[MRN] [nvarchar](50) NOT NULL,
	[ARIA_PatientSer] [int] NOT NULL,
	[ARIA_CourseSer] [int] NOT NULL,
	[CourseID] [nvarchar](50) NOT NULL,
	[CreationDateTime] [datetime] NULL,
	[db_entry_time] [datetime] NOT NULL,
	[db_entry_userid] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_DVHAnalysis_PlanSum] PRIMARY KEY CLUSTERED 
(
	[PlanSum_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


CREATE TABLE [dbo].[DVHAnalysis_Metric](
	[Metric_ID] [int] IDENTITY(1,1) NOT NULL,
	[Instance_ID] [int] NOT NULL,
	[PlanSum_ID] [int] NULL,
	[PlanSetup_ID] [int] NULL,
	[StructureID] [nvarchar](50) NOT NULL,
	[DVHMetricName] [nvarchar](50) NOT NULL,
	[DVHMetricValue] [float] NOT NULL,
	[StructureSetID] [nvarchar](50) NOT NULL,
	[IsReIrradiationEvaluation] [bit] NULL,
	[IsVolumetricDVHDoseEstimate] [bit] NULL,
	[IsPointDoseEstimate] [bit] NULL,
	[db_entry_time] [datetime] NOT NULL,
	[db_entry_userid] [nvarchar](50) NOT NULL,
	[Comment] [nvarchar](1024) NULL,
	[db_entry_source] [nvarchar](512) NULL,
 CONSTRAINT [PK_DVHAnalysis_Metric] PRIMARY KEY CLUSTERED 
(
	[Metric_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

