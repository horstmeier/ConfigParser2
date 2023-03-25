# ConfigParser2

The ConfigParser2 library is a C# library that adds a few features to the standard
JSON reader in .NET.

## Features

### References

With the library, you can reference other JSON values in your JSON file. This
is useful for when you have a lot of repeated values.

As an example, let's say you have a JSON file that looks like this:

    {
        "OutputDirectory": "C:\\Users\\User\\Documents\\Output\\",
        "InputDirectory": "C:\\Users\\User\\Documents\\Input\\"
    }

You can introduce a working directory variable, and then reference it in the
other variables:

    {
        "WorkingDirectory": "C:\\Users\\User\\Documents\\",
        "OutputDirectory": "${WorkingDirectory}Output\\",
        "InputDirectory": "${WorkingDirectory}Input\\"
    }

This will also work with hirarchical JSON files. For example, if you have a
JSON file that looks like this:

    {
        "WorkingDirectory": "C:\\Users\\User\\Documents\\",
        "OutputDirectory": "${WorkingDirectory}Output\\",
        "InputDirectory": "${WorkingDirectory}Input\\",
        "Files": [
            {
                "Name": "File1",
                "Path": "${InputDirectory}File1.txt"
            },
            {
                "Name": "File2",
                "Path": "${InputDirectory}File2.txt"
            }
        ]
    }

and also if the JSON file looks like this:

    {
        "WorkingDirectory": "C:\\Users\\User\\Documents\\",
        "OutputDirectory": "${WorkingDirectory}Output\\",
        "InputDirectory": "${WorkingDirectory}Input\\",
        "Files": {
            "File1": {
                "Path": "${InputDirectory}File1.txt"
            },
            "File2": {
                "Path": "${InputDirectory}File2.txt"
            }
        }
    }


### Environment Variables

With the library, you can reference environment variables in your JSON file.
This is useful for when you want to have a JSON file that is portable across
different machines.

You specify an environment variable by using the following syntax:

    ${ENV:VariableName}

As an example, let's say you have a JSON file that looks like this:

    {
        "OutputDirectory": "C:\\Users\\User\\Documents\\Output\\",
        "InputDirectory": "C:\\Users\\User\\Documents\\Input\\"
    }

You can introduce an environment variable, and then reference it in the other
variables:

    {
        "OutputDirectory": "${ENV:USERPROFILE}\\Documents\\Output\\",
        "InputDirectory": "${ENV:USERPROFILE}\\Documents\\Input\\"
    }

### File Contents

With the library, you can reference the contents of a file in your JSON file.

You specify a file by using the following syntax:

    ${FILE:Path}

As an example, let's say you have a JSON file that looks like this:

    {
        "OutputDirectory": "C:\\Users\\User\\Documents\\Output\\",
        "InputDirectory": "C:\\Users\\User\\Documents\\Input\\"
    }

You can introduce a file, and then reference it in the other variables:

    {
        "OutputDirectory": "${FILE:C:\\Users\\User\\Documents\\Output.txt}",
        "InputDirectory": "${FILE:C:\\Users\\User\\Documents\\Input.txt}"
    }

You can also add a regular expression to the file reference. The regular
expression will be applied to the file contents, and the first match will be
used as the value.

    {
        "OutputDirectory": "${FILE:C:\\Users\\User\\Documents\\Output.txt:.*}",
        "InputDirectory": "${FILE:C:\\Users\\User\\Documents\\Input.txt:.*}"
    }


### Current DateTime

With the library, you can reference the current date in your JSON file.

You specify the current date by using the following syntax:

    ${NOW}

or if you need the UTC date:

    ${UTCNOW}

### Base64 Decoding

With the library, you can decode a string to base64 in your JSON file.

You specify the base64 encoding by using the following syntax:

    ${BASE64:Value}

### Path Manipulation

With the library, you can combine paths in your JSON file. If you add a slash at the 
end of a replacement, the replacement value will always end in a directory separator.

You specify the path manipulation by using the following syntax:

    ${key/}

For example, if you have a JSON file that looks like this:

    {
        "WorkingDirectory": "C:\\Users\\User\\Documents",
        "OutputDirectory": "${WorkingDirectory/}Output\\"
    }

The value of OutputDirectory will be "C:\\Users\\User\\Documents\\Output\\".
The same is true if the file looks like this:

    {
        "WorkingDirectory": "C:\\Users\\User\\Documents\\",
        "OutputDirectory": "${WorkingDirectory/}Output\\"
    }


## Usage

You create a new ConfigParser2 object, and then call the Parse method on it.

    ConfigReader parser = new ConfigReader(content);
    var r = cr.Get("key1");

The return value can have theses types:

* StringValue
* NumberValue
* TrueValue
* FalseValue
* NullValue
* ArrayValue

You can access a subsection by using GetSection:

    var r = cr.GetSection("key1");

The return value will be null if the key doesn't exist, or if the key is not a
section. Othewise it will have the Type ConfigReader. So you can call Get or
GetSection on it.



