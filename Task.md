## Description
The input is a large text file, where each line is a Number. String

For example:

```
415. Apple
30432. Something something something
1. Apple
32. Cherry is the best
2. Banana is yellow
```

Both parts can be repeated within the file. You need to get another file as output, where all
the lines are sorted. Sorting criteria: String part is compared first, if it matches then
Number.
Those in the example above, it should be:
```
1. Apple
415. Apple
2. Banana is yellow
32. Cherry is the best
30432. Something something something
```

## Requirements
### You need to write two programs:
1. A utility for creating a test file of a given size. The result of the work should be a text file
of the type described above. There must be some number of lines with the same String
part.
2. The actual sorter. An important point, the file can be very large. The size of ~100Gb will
be used for testing.
When evaluating the completed task, we will first look at the result (correctness of
generation / sorting and running time), and secondly, at how the candidate writes the code.

### Limits and assumptions
1. String part length expected to be <= 100
2. Number part <= 10^18
3. Utility is not forced to generate exact file size

## Language
C# is preferable but not limited to. 
   
### Result format
We are expecting to receive a link to a GitHub repo with the completed task and well-described instructions on how to run it in Readme.md

In case you have some comments/notes you can also add them to Readme.md
