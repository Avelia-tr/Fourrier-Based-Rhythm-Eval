HOW TO USE:

`dotnet run -- args_listed_below`

this supports multiple arguement but each arguement support only 1 sub_arguement (unless precised so)

thanks misha for the contribution math wise of the last version :3

you need to set up the api via -s to use anything fecthing maps dynamically

 -s Api_id Api_secret
    creates a file storing the apikey/secret
    
 -m map_id
    compute the rating of a map
    
 -ms mapSet_id
    compute the rating of a mapSet
    
 -p id
    outputs every score in a player top plays ranked by complexity
    
 -f folder
    compute the rating for every map in a folder
    
 -t text_file
    parse a file and outputs the result of every maps following id 
    
 -tm text_file
    parse a file and outputs the result of every mapset following their id
    
 -o output_name
    output place if not precised nothing will be saved

current goals :
 variadic number of argument
