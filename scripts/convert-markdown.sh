#!/bin/bash

# Simple Markdown to HTML converter for GitHub Pages
# This script converts markdown files to HTML with basic styling

convert_markdown_to_html() {
    local input_file="$1"
    local output_file="$2"
    local title="$3"
    
    echo "<!DOCTYPE html>" > "$output_file"
    echo "<html lang='en'>" >> "$output_file"
    echo "<head>" >> "$output_file"
    echo "    <meta charset='utf-8'>" >> "$output_file"
    echo "    <meta name='viewport' content='width=device-width, initial-scale=1'>" >> "$output_file"
    echo "    <title>Easter Egg Hunt - $title</title>" >> "$output_file"
    echo "    <style>" >> "$output_file"
    echo "        body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; max-width: 1200px; margin: 0 auto; padding: 20px; line-height: 1.6; color: #333; }" >> "$output_file"
    echo "        h1, h2, h3, h4, h5, h6 { color: #2c3e50; margin-top: 2em; margin-bottom: 1em; }" >> "$output_file"
    echo "        h1 { border-bottom: 2px solid #3498db; padding-bottom: 0.5em; }" >> "$output_file"
    echo "        h2 { border-bottom: 1px solid #bdc3c7; padding-bottom: 0.3em; }" >> "$output_file"
    echo "        code { background-color: #f8f9fa; padding: 2px 4px; border-radius: 3px; font-family: 'Monaco', 'Menlo', 'Ubuntu Mono', monospace; }" >> "$output_file"
    echo "        pre { background-color: #f8f9fa; padding: 15px; border-radius: 5px; overflow-x: auto; }" >> "$output_file"
    echo "        pre code { background-color: transparent; padding: 0; }" >> "$output_file"
    echo "        blockquote { border-left: 4px solid #3498db; margin: 0; padding-left: 20px; color: #7f8c8d; }" >> "$output_file"
    echo "        table { border-collapse: collapse; width: 100%; margin: 1em 0; }" >> "$output_file"
    echo "        th, td { border: 1px solid #bdc3c7; padding: 8px 12px; text-align: left; }" >> "$output_file"
    echo "        th { background-color: #ecf0f1; font-weight: bold; }" >> "$output_file"
    echo "        ul, ol { padding-left: 2em; }" >> "$output_file"
    echo "        li { margin: 0.5em 0; }" >> "$output_file"
    echo "        a { color: #3498db; text-decoration: none; }" >> "$output_file"
    echo "        a:hover { text-decoration: underline; }" >> "$output_file"
    echo "        .nav { background-color: #2c3e50; color: white; padding: 1em; margin: -20px -20px 2em -20px; }" >> "$output_file"
    echo "        .nav a { color: white; margin-right: 1em; }" >> "$output_file"
    echo "        .footer { margin-top: 3em; padding-top: 2em; border-top: 1px solid #bdc3c7; color: #7f8c8d; font-size: 0.9em; }" >> "$output_file"
    echo "    </style>" >> "$output_file"
    echo "</head>" >> "$output_file"
    echo "<body>" >> "$output_file"
    
    # Navigation
    echo "    <div class='nav'>" >> "$output_file"
    echo "        <a href='index.html'>🏠 Home</a>" >> "$output_file"
    echo "        <a href='coverage/index.html'>📊 Coverage</a>" >> "$output_file"
    echo "        <a href='architecture.html'>🏗️ Architecture</a>" >> "$output_file"
    echo "        <a href='developer-guide.html'>👨‍💻 Developer Guide</a>" >> "$output_file"
    echo "        <a href='troubleshooting.html'>🔧 Troubleshooting</a>" >> "$output_file"
    echo "        <a href='sprint-planning.html'>📋 Sprint Planning</a>" >> "$output_file"
    echo "    </div>" >> "$output_file"
    
    # Convert markdown content (basic conversion)
    sed -E '
        # Headers
        s/^# (.*)$/<h1>\1<\/h1>/
        s/^## (.*)$/<h2>\1<\/h2>/
        s/^### (.*)$/<h3>\1<\/h3>/
        s/^#### (.*)$/<h4>\1<\/h4>/
        s/^##### (.*)$/<h5>\1<\/h5>/
        s/^###### (.*)$/<h6>\1<\/h6>/
        
        # Bold and italic
        s/\*\*(.*?)\*\*/<strong>\1<\/strong>/g
        s/\*(.*?)\*/<em>\1<\/em>/g
        
        # Code blocks
        s/^```(.*)$/<pre><code class="language-\1">/
        s/^```$/<\/code><\/pre>/
        
        # Inline code
        s/`([^`]+)`/<code>\1<\/code>/g
        
        # Links
        s/\[([^\]]+)\]\(([^)]+)\)/<a href="\2">\1<\/a>/g
        
        # Lists
        s/^\- (.*)$/<li>\1<\/li>/
        s/^[0-9]+\. (.*)$/<li>\1<\/li>/
        
        # Line breaks
        s/^$/<br\/>/
        
        # Blockquotes
        s/^> (.*)$/<blockquote>\1<\/blockquote>/
    ' "$input_file" >> "$output_file"
    
    # Footer
    echo "    <div class='footer'>" >> "$output_file"
    echo "        <p>Generated on $(date -u +'%Y-%m-%d %H:%M:%S UTC') | Easter Egg Hunt Project</p>" >> "$output_file"
    echo "    </div>" >> "$output_file"
    
    echo "</body>" >> "$output_file"
    echo "</html>" >> "$output_file"
}

# Convert all markdown files
for file in docs/*.md; do
    if [ -f "$file" ]; then
        filename=$(basename "$file" .md)
        title=$(echo "$filename" | sed 's/-/ /g' | sed 's/\b\w/\U&/g')
        convert_markdown_to_html "$file" "docs/html/${filename}.html" "$title"
        echo "Converted $file to docs/html/${filename}.html"
    fi
done

echo "All markdown files converted to HTML!"
