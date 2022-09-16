# Page Parser. C# say'so "Help me"

This project was created on a test task. Challenge: [site](https://www.ilcats.ru/toyota/?function=getModels&market=EU) parsing and entering data into SQL db. The project uses: Entity Framework, AngleSharp. The solution will be divided into several projects: Parsing part and database part.


## Parsing part

The [first layer](https://www.ilcats.ru/toyota/?function=getModels&market=EU) of the site has a list of car models. A task: get name, id, dateRange (start date and finish date of model production) and model codes.

### First Layer

At this level, the necessary data is in the following structure:
```html
<!--parent construct-->
<div class="List">
  <div class="Header">
    <div class="name">name of model</div>
  </div>
  <!--children construct-->
  <div class="List " style="display: block;">
    <div class="List">
      <div class="id">
        <a href="URL" title="" target="">id of model</a>
      </div>
      <div class="dateRange">date start&nbsp;-&nbsp;date finish</div>
      <div class="modelCode">list of model codes</div>
    </div>
  </div>
</div>
```

Problems:
- This construction is not named in any way, so it must be selected using child objects.
- Several models can be written in one such construction.

Notes:
- The "parent" word will refer to the entire structure presented above.
- The "children" word will refer to the list of models inside the parent.

