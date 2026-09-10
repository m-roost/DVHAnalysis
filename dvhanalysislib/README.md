# DVH Analysis Library

*This file is best viewed with the "Markdown Editor" extension
in Visual Studio.*

## Concepts

The main concept to understand is that a *metric*
is a computation derived from a *DVH* (dose-volume histogram).
For example, given a DVH curve, the metric D50%[Gy] computes the
(absolute) dose at the 50% (relative) volume value on the curve.

The dose and the volume in a DVH may be in absolute units
(e.g., Gy for dose and cc for volume)
or in relative units (percent).
In addition, the dose in a DVH may represent physical dose
or biological dose (a.k.a. *biodose*).
The biodose can be computed using different *models*,
two of which are the LQ and LQL models.

Thus, for any DVH metric there are two computations involved.
First, the correct DVH must be generated from the specified model
(e.g., physical or LQ biodose).
Then, the desired metric is calculated using the generated DVH.

The DVH model and metric are encapsulated by the class
`DVHMetricSetup`, which contains the desired `DVHModel` and `Metric`.
Once those properties are specified, the `Calculate`
method of `DVHMetricSetup` can be called
to obtain the result (as a `MetricResult` object).

The `DVHModel` class is the base class to the
`StandardDVHModel` (physical dose),
`LQBioDoseDVHModel` (LQ biodose),
and `LQLBioDoseDVHModel` (LQL biodose) classes.
Its `Calculate` method takes the required data from Eclipse
(e.g., plan and structure) and returns a `DVH` object.
This `DVH` object contains the DVH curve,
the dose and metric units, and some common, precalculated metrics.

The `Metric` class is the base class to many metric classes,
such as `MeanDoseMetric`, `DoseToVolumeMetric`,
and `NTCPMetric`.
Its `Calculate` method takes a `DVH` object
and returns a `MetricResult` object
containing the result value and its units.
It's up to the derived classes of `Metric`
to use the DVH appropriately to calculate
the metric they represent.

## Cache

Calculating the DVH based on the biodose can be a costly process.
Though it may take under a second for any single DVH,
calculating many DVHs can be noticibly slow.

In some cases, different metrics for the same DVH are desired.
Therefore, it would be more efficient to calculate the DVH once
and reuse it for multiple metrics.

This reuse is accomplished via a cache.
The `Calculate` method of the `DVHModel` class
checks if the requested DVH has been previously calculated.
If so, it returns the cached DVH;
otherwise, it calculates the DVH and stores in it the cache.

The cache is represented by the `DVHCache` class.
It creates a default cache as a static property called `DefaultCache`.
The cache may be cleared by calling the `Clear` method.
This method should be called when the stored DVHs
are outdated (for example, when the patient is re-opened
and there is the chance that a plan's dose was recalculated).
