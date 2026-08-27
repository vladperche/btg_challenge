
## Prompt: Cluster assignment logic

**Tool**: Google Antigravity

**What I Asked:**
I splitted the prompts on:
- Architeture definition
- Docker requisites
- Persistence requisites
- Presentation requisites
- Each model on a specific prompt
- Then, finally, the bussiness rule prompt with the steps to check the customer's credit

**What happened:**
Since I organized the information into blocks by prompt, the context created by the AI ​​was very concise and consistent with the expected result.

**Final solution:**
Putting all _Business Rules_ into **tables**, created a fully customizable application without the need to change/rebuild _Source Code_.
Letting the user in **full control** of credit approval rules, just _rewriting_ the rules.


##  FULL LOG
-- First prompt --

# Archictecure definition

Build a REST API for a local prototyping Environment, using the Docker containers, with the following architecture:

- Docker container #1: Presentation layer, with .Net Core C# 10.0
- Docker container #2: REDIS, for "cache-aside"
- Docker container #3: MongoDB for data persistense

## Docker requisites

- To split applications from data, for each Docker instance, configure them to use the `${workspaceFolder}/dockerdata/${volumName}` to preserve persistense even if the containers are stopped or deleted.

- The presentation container **must have** _network acess_ to the persistence containers. Or, the persistence containers **must allow** _network access_ from the presentation container.

- Create the script to start/run all Docker containers.

## Persistence requisites

Configure REDIS and MongoDB to have a password (or user/password, or a key) to simulate the Production Environment security issues.

## Presentation requisites

Create a .Net Core C# 10.0 solution, and group the C# projects with the following structure:

- API: a webapi project for presentation layer, to setup the Web Application, and handle:
    - Application setup
    - Security configuration
    - Controllers
    - Swagger definitions
    - At least one first Controller, to handle the Health/Check endpoint
- entities: a classlib project, to define/handle:
    - Data Models
    - Interfaces for Dependency Injection
    - Domains
    - Enumerables
    - Static Classes for public constants
    - Resources (RESX files for string constansts)
- corsscutting: a classlib project to define/handle:
    - database configuration
    - security configuration
    - dependecy injection services configurations
    - Filters and/or HttpFilters (if needed)
- services: a classlib project to define/handle:
    - check Entities/Models fields/data to keep rules independent from the persistence layer
    - inject the repositories instances to persist data independent from the database
- repositories: a classlib project to define/handle:
    - Entity framework configuration for MongoDB
    - for each Model defined in the project, create a correponding <ModelName>Repository.cs file to handle all persistence operations
    - for each <ModelName>Repository class, it MAY or MAY NOT handle CACHE-ASIDE (in REDIS), according to the data access frequency

-- new iteration --

# Minor Ajustment

- In the API/presentation: setup the default configuration, for all JSON parse/stringify to use the lowercase snake case
- In the Entities/Models: preserve the CamelCase names

-- new iteration --

# Entity Definition

Create a Model class for the **Market Debt Types**, with the following fields:
- Value: string, primary key, required (not null), a string defined by the user with the id/code of this record
- Meaning: string, required (not null), the meaningfull description of this record

In the corresponding Repository, use the Caching check.
In the corresponding Controller, create the following endpoints:
    - POST: receive a JSON in the Body of the request, and INSERT it on persistence, if the "value" key already exists, update.
    - GET: return a list of all records
    - GET/{value}: return a specific value, if exists
    - DELETE/{value}: delete a specific record, if exists

-- new iteration --

# Entity Definition

Create a Model class for the **Job Title Category**, with the following fields:
- Category: string, primary key, required (not null), a string defined by the user, with the id/code of this record
- Priority: integer, required (not null), positive greater than zero
- Multiplier: decimal, required (not null), positive greater or equal to zero
- Keywords: string[], optional (if null, set the array to an "empty array" []), but, if there's a value, must be not empty nor null

In the corresponding Repository, use the Caching check.
In the corresponding Controller, create the following endpoints:
    - POST: receive a JSON in the Body of the request, and INSERT it on persistence, if the "Category" key already exists, update.
    - GET: return a list of all records
    - GET/{category}: return a specific value, if exists
    - DELETE/{category}: delete a specific record, if exists

-- new iteration --

# Entity Definition

Create a model class for the **Customer Cluster**, with the following fields:
- Cluster ID: string, primary key, required (not null), a string defined by the user, with the id/code of this record
- Priority: integer, required (not null), positive greater than zero
- Name: string, required (not null), a meaningfull description of the record
- Socre: decimal, required (not null), positive equal or greater than zero
- Age Min: integer, required (not null), positive equal or greater than zero
- Age Max: integer, required (not null), positive greater than zero
- Debt Condition Market Debt Check: boolean, optional, default: false
- Debt Condition Market Types: string[], optional (if null, set the array to "emppty array" []), if there's a value: check the existence in **Market Debt Type** "value" key.
- Base Limit: decimal, required (not null), positive equal or greater than zero
- Cap Limit: decimal, required (not null), positive equal or greater than zero

In the corresponding Repository, use the Caching check.
In the corresponding Controller, create the following endpoints:
    - POST: receive a JSON in the Body of the request, and INSERT it on persistence, if the "Cluster ID" key already exists, update.
    - GET: return a list of all records
    - GET/{cluster}: return a specific value, if exists
    - DELETE/{cluster}: delete a specific record, if exists

-- new iteration --

# Entity Definition

Create a model for **Monthly Income**, with the following fields:
- Category: string, required (not null), must check existence in **Job Title Category**
- Cluster ID: string, required (not null), must check existence in **Customer Cluster**
- Income: decimal, required (not null), positive equal or greater than zero.
[The Primary Key is defined by the "pair" {Category, Cluster ID}]

In the corresponding Repository, use the Caching check.
In the corresponding Controller, create the following endpoints:
    - POST: receive a JSON in the Body of the request, and INSERT it on persistence, if the "Category" + "Cluster ID" key already exists, update.
    - GET: return a list of all records
    - GET/{category}: return a list of values, corresponding to the {category}
    - DELETE/{category}/{cluster}: delete a specific record, if exists

-- new iteration --

# Entity Definition

Create a model for **Penalty Rules**, with the following fields:
- Rule ID: string, primary key, required (not null), a string defined by the user, with the id/code of this record
- Priority: integer, required (not null), positive equal or greather than zero
- Trigger: string[], optional (if null, set the array to "emppty array" []), if there's a value: check the existence in **Market Debt Type** "value" key
- Effect: decimal, required, positive greater than zero.

In the corresponding Repository, use the Caching check.
In the corresponding Controller, create the following endpoints:
    - POST: receive a JSON in the Body of the request, and INSERT it on persistence, if the "Rule ID" key already exists, update.
    - GET: return a list of all records
    - GET/{rule_id}: return a specific value, if exists
    - DELETE/{rule_id}: delete a specific record, if exists

-- new iteraton --

# Entity Definition

Create a model for **Customer**, with the following fields:
- Id: string, primary key, unique identifier (ignore user input data, and generate a Random UUID)
- Name: string, required (not null), Full Name
- Age: integer, required, positive equal or greater than zero, Age in years
- Score: integer, optional, if null consider 0, equal or greater than zero, equal or less than 1000
- Has Market Debt: boolean, Whether the customer has any recorded market debt
- Market Debt Types: string[], optional (if null, set the array to "empty array" []), if there's any value, check **Market Debt Types** "value" key existence
- Location.City: string, required (not null), City of residence
- Location.State: string, required (not null), State abreviation (e.g. SP, RJ, etc)
- Location.Region: string, required (not null), possible values: [Norte, Nordeste, Centro-Oeste, Sudeste, Sul]
- Job Title: string, required (not null), Free-text job title

In the corresponding Repository, DO NOT use the caching check.
In the corresponding Controller, create the following endpoint:
    - POST("classify"): receive a JSON of the model in the Body of the request, then, enrich the received data with the following fields:
        - Job Category: 
            - Search in the **Job Title Categories** if the `job_title_categories.keywords[]` matches case-insentively anywhere in the `customer.job_title`, if `job_title_categories.keywords[]`==[] matchs any, ordered by `job_title_categories.priority` (ascending), fetch the first record that has a match
            - Save the fields:
                - `customer.job_category` == `job_title_categories.category`
                - `customer.job_multiplier` == `job_title_categories.multiplier`
        - Customer Cluster: search in the **Customer Cluster**, ordered by `customer_cluster.priority` (ascending), the first record that matches:
            - `customer.score` >= `customer_cluster.score`
            - `customer.age` >= `customer_cluster.age_min`
            - `customer.age` <= `customer_cluster.age_max` OR `customer_cluster.age_max` == 0
            - if `customer_cluster.debt_condition_market_debt_check` == true, then `customer.has_market_debt` MUST BE FALSE
            - if `customer_cluster.debt_condition_market_types` IS NOT [], then `customer.market_debt_types[]` MUST NOT MATCH any values
            - fetch the first record that match all these conditions
            - save the fields:
                - `customer.customer_cluster` = `customer_cluster.cluster_id`
                - `customer.cluster_name` = `customer_cluster.name`
                - `customer.base_limit` = `customer_cluster.base_limit`
                - `customer.cap_limit` = `customer_cluster.cap_limit`
        - Monthly Income:
            - Search for the first record in **Monthly Incomes** that matchs:
                - `customer.job_category` == `monthly_incomes.category`
                - `customer.customer_cluster` == `monthly_incomes.cluster_id`
            - Save the value:
                - `customer.monthly_income` == `monthly_incomes.income`
        - Penalty Factor:
            - Search in **Penalty Rules** if theres ANY MATCH between `penalty_rules.trigger[]` exists in `customer.market_debt_types[]`
            - If found, save the value:
                - `customer.penalty_factor` = `penalty_rules.effect`
            - If NOT FOUND, save the value:
                - `customer.penalty_factor` = 1.0
        - Approved Limit:
            - Save the value:
                - `customer.approved_limit` = round_to_neares_100(
                    min( `customer.base_limit` * `customer.job_multiplier` * `customer.penalty_factor`, `customer.cap_limit` )
                )
        
        = The PERSISTED DATA and the RESPONSE DATA should be: { ...received data, ...enriched fields }

# Minor adjustments

In the **Customer** Controller, on the `POST("classify")` Request Body, the "enriched" (calculated fields) should not be required.
These fields should only be added AFTER the REQUEST and BEFORE the RESPONSE.

# Unity test

Build a Unit and Integration tests are required:
- **Unity test** must cover the core classification logic in isolation:
    - Cluster assignment for each customer, including boundary conditions (e.g. score exactly at threshold)
    - Job category matching including case-insensitive and priority ordering
    - Credit limit calculation: base formula, penalty application, cap enforcement, and `round_to_nearest_100`
    - Monthly income lookup for all clusters x job category combinations
    - CLUSTER_D denial (approved_limit = 0)
- **Integration test** must exercise the full request/response cycle:
    - POST /customers/classify with valid inputs returns correct output contract
    - POST /customers/classify with invalid or missing fields returns appropriate error responses

The test suit must be runnable with a single command (e.g. `dotnet test`).

