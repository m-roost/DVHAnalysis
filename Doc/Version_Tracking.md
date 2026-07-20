
# Version Updates Tracking

Only the following recent versions are included:

- [DVH Analysis v3.5.1.x](#dvh-analysis-v351x)
- [DVH Analysis v3.5.0.x](#dvh-analysis-v350x)
- [DVH Analysis v3.4.1.x](#dvh-analysis-v341x)
- [DVH Analysis v3.4.0.x](#dvh-analysis-v340x)



<div style="page-break-after: always;margin-bottom:120px;"></div>


# <h1 style="text-align: center;">DVH Analysis v3.5.1.x</h1>


Replace NaN with simply leaving cells blank, in both Main view window and Save to DB window.  


<div style="text-align: center;">
    <img src="./images/LeaveNaNblank_mainView.png" alt="Sample Image" width="500" style="border: 2px solid gray;"/>
</div>

<br>

<div style="text-align: center;">
    <img src="./images/LeaveNaNblank_SaveToDB.png" alt="Sample Image" width="500" style="border: 2px solid gray;"/>
</div>


<div style="page-break-after: always;margin-bottom:120px;"></div>






# <h1 style="text-align: center;">DVH Analysis v3.5.0.x</h1>

**Bug Fix:** Sync all referenced assembly versions to 3.5.0.2. Since Eclipse may load previous version (1.0.0.0) of AriaDb.Sql.

These referenced dll version may stay at 3.5.0.2 for a while as long as they are not changed. So DVH_Analysis version may get larger than its referenced dll assemble versions in the future.

<div style="page-break-after: always;margin-bottom:120px;"></div>






# <h1 style="text-align: center;">DVH Analysis v3.4.1.x</h1>

- **Bug Fix:** Fix column order discrepancy (between main and SaveToDB window):
  
<div style="text-align: center;">
    <img src="./images/Column_order_discripency.png" alt="Bug example" width="400" style="border: 2px solid gray;"/>
</div>
<p style="text-align: center;"><em>Error example</em></p>
  
<br>



- Comment field in SaveToDB window datagrid can have text wrapped:

<div style="text-align: center;">
    <img src="./images/Comment_wrap.png" alt="Sample Image" width="500" style="border: 2px solid gray;"/>
</div>
<br>

<div style="page-break-after: always;margin-bottom:120px;"></div>







# <h1 style="text-align: center;">DVH Analysis v3.4.0.x</h1>

- Two Point Dose Metrics (which takes manual type-in values) have been added:


<div style="text-align: center;">
    <img src="./images/PointDoseMetrics.png" alt="Sample Image" width="350" style="border: 2px solid gray;"/>
</div>
<br>
<div style="text-align: center;">
    <img src="./images/SMPC_inputWindow.png" alt="Sample Image" width="350" style="border: 2px solid gray;"/>
</div>
<br>

<div style="page-break-after: always;margin-bottom:120px;"></div>

- PDF file can be created and uploaded to Aria Patient Document from the Save to Database window:

<div style="text-align: center;">
    <img src="./images/SaveToDBWindowWithCommentAndGreySelections.png" alt="Sample Image" width="600" style="border: 2px solid gray;"/>
</div>
<br>

<div style="text-align: center;">
    <img src="./images/SaveToDB_PDF.png" alt="Sample Image" width="600" style="border: 2px solid gray;"/>
</div>
<br>

- Two new columns, Comment and db_entry_source, have been added in the Metric table:

<div style="text-align: center;">
    <img src="./images/DB_table.png" alt="Sample Image" width="800" style="border: 2px solid gray;"/>
</div>
<br>


<div style="page-break-after: always;margin-bottom:120px;"></div>




