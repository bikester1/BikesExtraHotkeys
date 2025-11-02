#!/usr/bin/env python3
"""
Version Updater Script
Reads version information from a CSV file and updates version numbers across specified files.
"""

import csv
import re
import os
from pathlib import Path
from typing import Dict, List, Tuple

class VersionUpdater:
    def __init__(self, csv_file: str):
        """Initialize with CSV file containing version information."""
        self.csv_file = csv_file
        self.version_locations = []
        self.load_version_data()
    
    def load_version_data(self):
        """Load version locations from CSV file."""
        try:
            with open(self.csv_file, 'r', newline='', encoding='utf-8') as file:
                reader = csv.DictReader(file)
                self.version_locations = list(reader)
                print(f"Loaded {len(self.version_locations)} version locations from {self.csv_file}")
        except FileNotFoundError:
            print(f"Error: CSV file '{self.csv_file}' not found.")
            self.create_sample_csv()
        except Exception as e:
            print(f"Error reading CSV file: {e}")
    
    def create_sample_csv(self):
        """Create a sample CSV file for reference."""
        sample_data = [
            {'file_path': 'setup.py', 'pattern': '__version__\\s*=\\s*["\']([^"\']+)["\']', 'display_name': 'setup.py version'},
            {'file_path': 'package.json', 'pattern': '"version"\\s*:\\s*"([^"]+)"', 'display_name': 'package.json version'},
            {'file_path': 'config.yaml', 'pattern': 'version:\\s*(.+)', 'display_name': 'config.yaml version'},
        ]
        
        with open(self.csv_file, 'w', newline='', encoding='utf-8') as file:
            fieldnames = ['file_path', 'pattern', 'display_name']
            writer = csv.DictWriter(file, fieldnames=fieldnames)
            writer.writeheader()
            writer.writerows(sample_data)
        
        print(f"Created sample CSV file: {self.csv_file}")
        print("Please edit this file to match your actual file paths and version patterns.")
    
    def find_current_versions(self) -> Dict[str, str]:
        """Find current version numbers in all specified files."""
        current_versions = {}
        
        for location in self.version_locations:
            file_path = location['file_path']
            pattern = location['pattern']
            display_name = location.get('display_name', file_path)
            
            if not os.path.exists(file_path):
                print(f"Warning: File not found - {file_path}")
                current_versions[display_name] = "NOT FOUND"
                continue
            
            try:
                with open(file_path, 'r', encoding='utf-8') as file:
                    content = file.read()
                
                match = re.search(pattern, content)
                if match:
                    current_version = match.group(1)
                    current_versions[display_name] = current_version
                else:
                    current_versions[display_name] = "NOT FOUND"
                    print(f"Warning: Version pattern not found in {file_path}")
            
            except Exception as e:
                print(f"Error reading {file_path}: {e}")
                current_versions[display_name] = "ERROR"
        
        return current_versions
    
    def update_version(self, new_version: str) -> List[Tuple[str, bool, str]]:
        """Update version number in all files."""
        results = []
        
        for location in self.version_locations:
            file_path = location['file_path']
            pattern = location['pattern']
            display_name = location.get('display_name', file_path)
            
            if not os.path.exists(file_path):
                results.append((display_name, False, "File not found"))
                continue
            
            try:
                with open(file_path, 'r', encoding='utf-8') as file:
                    content = file.read()
                
                # Replace the version number while preserving the pattern structure
                def replace_version(match):
                    # Get the full matched text
                    full_match = match.group(0)
                    # Get capture groups (everything except the version number)
                    groups = match.groups()
                    if groups:
                        # Replace the first capture group (version number)
                        return full_match.replace(groups[0], new_version, 1)
                    else:
                        # Fallback: simple replacement
                        return full_match
                
                updated_content = re.sub(pattern, replace_version, content)
                
                if updated_content != content:
                    with open(file_path, 'w', encoding='utf-8') as file:
                        file.write(updated_content)
                    results.append((display_name, True, f"Updated to {new_version}"))
                else:
                    results.append((display_name, False, "No changes needed"))
            
            except Exception as e:
                results.append((display_name, False, f"Error: {e}"))
        
        return results
    
    def show_current_versions(self):
        """Display current version numbers."""
        print("\n" + "="*60)
        print("CURRENT VERSION STATUS")
        print("="*60)
        
        current_versions = self.find_current_versions()
        
        for display_name, version in current_versions.items():
            status = "✓ Found" if version not in ["NOT FOUND", "ERROR"] else "✗ Missing"
            print(f"{display_name:30} | {status:10} | {version}")
        
        return current_versions
    
    def get_new_version(self):
        """Prompt user for new version number."""
        print("\n" + "="*60)
        print("VERSION UPDATE")
        print("="*60)
        
        while True:
            new_version = input("Enter new version number: ").strip()
            
            if not new_version:
                print("Version cannot be empty. Please try again.")
                continue
            
            # Basic version format validation
            version_pattern = r'^\d+\.\d+\.\d+(\-\w+)*(\.\w+)*$|^v?\d+\.\d+\.\d+(\-\w+)*(\.\w+)*$'
            if not re.match(version_pattern, new_version):
                print("Warning: This doesn't look like a standard version number.")
                print("Examples: 1.0.0, 2.1.3, 1.0.0-beta, v1.0.0")
                confirm = input("Continue anyway? (y/N): ").strip().lower()
                if confirm != 'y':
                    continue
            
            return new_version
    
    def run(self):
        """Main execution method."""
        print("Version Updater Tool")
        print("This tool will help you update version numbers across multiple files.")
        
        # Show current versions
        current_versions = self.show_current_versions()
        
        # Check if any versions were found
        found_versions = [v for v in current_versions.values() if v not in ["NOT FOUND", "ERROR"]]
        
        if not found_versions:
            print("\nNo versions found to update. Please check your CSV file and file paths.")
            return
        
        # Get new version
        new_version = self.get_new_version()
        
        # Confirm before updating
        print(f"\nYou are about to update version numbers to: {new_version}")
        confirm = input("Do you want to continue? (y/N): ").strip().lower()
        
        if confirm != 'y':
            print("Update cancelled.")
            return
        
        # Update versions
        print("\nUpdating versions...")
        results = self.update_version(new_version)
        
        # Show results
        print("\n" + "="*60)
        print("UPDATE RESULTS")
        print("="*60)
        
        for display_name, success, message in results:
            status = "✓" if success else "✗"
            print(f"{status} {display_name:30} | {message}")
        
        # Show final versions
        print("\n" + "="*60)
        print("FINAL VERSION STATUS")
        print("="*60)
        
        final_versions = self.find_current_versions()
        for display_name, version in final_versions.items():
            status = "✓ Updated" if version == new_version else "✗ Issue"
            print(f"{display_name:30} | {status:10} | {version}")


def main():
    """Main function to run the version updater."""
    # Default CSV file name
    csv_file = "version_locations.csv"
    
    # Check if CSV file exists, if not, let user specify
    if not os.path.exists(csv_file):
        print(f"CSV file '{csv_file}' not found in current directory.")
        csv_file = input("Enter path to your CSV file (or press Enter to create sample): ").strip()
        
        if not csv_file:
            csv_file = "version_locations.csv"
    
    try:
        updater = VersionUpdater(csv_file)
        updater.run()
    except KeyboardInterrupt:
        print("\n\nOperation cancelled by user.")
    except Exception as e:
        print(f"\nAn error occurred: {e}")


if __name__ == "__main__":
    main()