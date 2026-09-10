


USE [ROARFormEntry] -- this in server of MROAR ntsrodbsdv1
GO



select count(*) as Instance from DVHAnalysis_DataEnterInstance
select count(*) as PlanSum from DVHAnalysis_PlanSum
select count(*) as PlanSetup from DVHAnalysis_PlanSetup
select count(*) as Metric from DVHAnalysis_Metric

-- display the latest recorded metrics 
select top 300 ps.Instance_ID, ps.PlanSetupID, m.StructureID, m.DVHMetricName, m.DVHMetricValue, m.IsReIrradiationEvaluation, ps.db_entry_time, m.Comment, m.db_entry_source, ins.db_entry_userid from DVHAnalysis_Metric m inner join DVHAnalysis_PlanSetup ps on m.PlanSetup_ID = ps.PlanSetup_ID inner join DVHAnalysis_DataEnterInstance ins on ins.Instance_ID = m.Instance_ID order by m.db_entry_time desc

select * from DVHAnalysis_DataEnterInstance order by db_entry_time desc


ALTER TABLE DVHAnalysis_Metric 
	ADD Comment NVARCHAR(1024) NULL,
    	db_entry_source NVARCHAR(512) NULL;


-- add new column 


--delete from DVHAnalysis_DataEnterInstance
--delete from DVHAnalysis_PlanSum
--delete from DVHAnalysis_PlanSetup
--delete from DVHAnalysis_Metric

--drop table DVHAnalysis_DataEnterInstance
--drop table DVHAnalysis_PlanSum
--drop table DVHAnalysis_PlanSetup
--drop table DVHAnalysis_Metric


CREATE TABLE [dbo].[DVHAnalysis_DataEnterInstance](
	[Instance_ID] [int] IDENTITY(1,1) NOT NULL,

	[MRN] [nvarchar](50) NOT NULL,

	[IsReIrradiationEvaluation] [bit] NULL,

	[db_entry_time] [datetime] NOT NULL,
	[db_entry_userid] [nvarchar](50) NOT NULL,

	--[TemplateUsed] [nvarchar](250) NULL,

 CONSTRAINT [PK_DVHAnalysis_DataEnterInstance] PRIMARY KEY CLUSTERED 
(
	[Instance_ID] ASC
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



CREATE TABLE [dbo].[DVHAnalysis_PlanSetup](
	[PlanSetup_ID] [int] IDENTITY(1,1) NOT NULL,
	[Instance_ID] [int] NOT NULL,
	[PlanSetupID] [nvarchar](50) NOT NULL,
	
	[MRN] [nvarchar](50) NOT NULL,
	[ARIA_PatientSer] [int] NOT NULL,

	[ARIA_CourseSer] [int] NOT NULL,
	[CourseID] [nvarchar](50) NOT NULL,
	[ARIA_PlanSetupSer] [int] NULL,

	[PlanSum_ID] [int] Null,
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


select Component_Weight, count(*) from dbo.DVHAnalysis_PlanSetup GROUP by Component_Weight -- all 0

select Component_NFractionsPlanned, count(*) as count from dbo.DVHAnalysis_PlanSetup GROUP by Component_NFractionsPlanned  order by count desc

select Component_NFractionsDelivered, count(*) as count from dbo.DVHAnalysis_PlanSetup GROUP by Component_NFractionsDelivered order by count desc  -- 1098

select IsVolumetricDoseComponent, count(*) as count from dbo.DVHAnalysis_PlanSetup GROUP by IsVolumetricDoseComponent order by count desc  -- all 1

select IsPaperChartBasedComponent, count(*) as count from dbo.DVHAnalysis_PlanSetup GROUP by IsPaperChartBasedComponent order by count desc -- all 0



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
 CONSTRAINT [PK_DVHAnalysis_Metric] PRIMARY KEY CLUSTERED 
(
	[Metric_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

select DVHMetricName, count(*) as count from dbo.DVHAnalysis_Metric GROUP by DVHMetricName order by count desc
select StructureID, count(*) as count from dbo.DVHAnalysis_Metric GROUP by StructureID order by count desc
select IsReIrradiationEvaluation, count(*) as count from dbo.DVHAnalysis_Metric GROUP by IsReIrradiationEvaluation order by count desc
select IsVolumetricDVHDoseEstimate, count(*) as count from dbo.DVHAnalysis_Metric GROUP by IsVolumetricDVHDoseEstimate order by count desc -- all 1
select IsPointDoseEstimate, count(*) as count from dbo.DVHAnalysis_Metric GROUP by IsPointDoseEstimate order by count desc -- all 0
