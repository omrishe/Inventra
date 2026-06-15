## role & permissions:

- each user should have permission table in jwt, backend only checks the permission needed for each endpoint
- role functions as both an identity of what each user is, and what default permissions are set
- a user's permission are mutable, they can be added or removed by a user with special permissions

## inventory:

### decision 1:

all inventory is held in the same database

#### the danger

incase of bugs (like if by accident you write dbContext.Products.ToList() it returns all the inventory of all the stores)

#### fix:

to mitigate the risk we use EF Core Global Query Filters on ChainId: it injects a hidden WHERE on chainId = CurrentUser.ChainId

#### reason

since this system is designed to be a mimic of an actual fully production Saas System there could be 50 stores, creating for each is alot of maintenance and issues

### decisions 2:

TPT (Table Per Type) use in database for inventory

#### reason

we would use polymorphism extensivly (Perishable,refridgerated,clothing... items), using TPH (Table Per Hierchy) would make querying better but would result in alot columns and can return alot of null values which are not needed

#### trade offs

more joins, meaning slower read and write

## concurrency

### decision 1:

i decided to name the different methods and when to use for the agent

#### reason

we use the db as the single source of truth since we want to prevent race conditions (2 different threads trying to change the same row) issues i added a list of fixes for race conditions in db and instructed the agent on where to use each fix

#### tradeoffs

there might be a better fix out of the list of fixes i gave which was used for a specific case
