# Random Media Generator

<p>Generates a random set of PDF files in a zipped up file which contains a manifest that will place that media on the provided accounts.</p>

# Running

<code>dotnet run [number of files] [... account numbers]</code>

<hr>
<p>For example this would generate a zip with 5 pdfs alternating as being assigned to account 1000 and account 1001.</p>
<code>dotnet run 5 1000 1001</code>
