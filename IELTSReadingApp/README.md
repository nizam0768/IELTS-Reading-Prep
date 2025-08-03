# IELTS Academic Reading Practice Application

A comprehensive .NET Blazor Server application designed to help students practice for the IELTS Academic Reading test. This application provides authentic practice tests with all 11 question types found in the actual IELTS exam.

## 🌟 Features

### Test Format
- **60-minute timed tests** (authentic IELTS timing)
- **3 reading passages** per test (2150-2750 words total)
- **40 questions** covering all IELTS question types
- **Band score calculation** (1-9 scale) based on official IELTS scoring

### Question Types Supported
1. **Multiple Choice** - Choose the correct answer from 4 options
2. **True/False/Not Given** - Identify factual accuracy
3. **Yes/No/Not Given** - Identify writer's opinions/claims
4. **Matching Information** - Match information to paragraphs
5. **Matching Headings** - Match headings to paragraphs/sections
6. **Matching Features** - Match features to categories
7. **Matching Sentence Endings** - Complete sentences logically
8. **Sentence Completion** - Fill in missing words
9. **Summary/Note/Table/Flow-chart Completion** - Complete summaries
10. **Diagram Label Completion** - Label diagrams
11. **Short-answer Questions** - Answer questions briefly

### Practice Tests Available
1. **Climate Change and Renewable Energy** (Intermediate)
2. **Ancient Civilizations and Archaeological Discoveries** (Advanced)
3. **Artificial Intelligence and Machine Learning** (Advanced)
4. **Marine Ecosystems and Conservation** (Intermediate)
5. **Smart Cities and Urban Development** (Intermediate)
6. **Cognitive Psychology and Memory** (Advanced)
7. **Space Exploration and Astronomy** (Intermediate)
8. **Modern Medicine and Public Health** (Advanced)
9. **World Literature and Cultural Impact** (Beginner)
10. **Global Economics and Trade** (Intermediate)

### User Interface Features
- **Card-based dashboard** for easy test selection
- **Split-screen test interface** (passage on left, questions on right)
- **Real-time timer** with visual countdown
- **Progress tracking** showing completion status
- **Responsive design** works on desktop and mobile
- **Modern Bootstrap 5 styling** with Font Awesome icons
- **Filter by difficulty level** and search by category
- **Vocabulary definitions** provided for complex terms

### Test Experience
- **Pre-test instructions** explaining format and timing
- **Section navigation** between reading passages
- **Auto-save answers** as you type/select
- **Visual progress indicator** showing completion percentage
- **Immediate results** with detailed band score breakdown
- **Answer review** capability (planned feature)
- **Retake functionality** to practice multiple times

## 🚀 Getting Started

### Prerequisites
- .NET 8.0 SDK or later
- Modern web browser (Chrome, Firefox, Safari, Edge)

### Installation

1. **Clone or download the project**
   ```bash
   git clone <repository-url>
   cd IELTSReadingApp
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the application**
   ```bash
   dotnet build
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

5. **Open in browser**
   - Navigate to `https://localhost:5001` or `http://localhost:5000`
   - The application will display the practice test dashboard

## 🏗️ Architecture

### Technology Stack
- **Framework**: .NET 8.0 Blazor Server
- **UI Framework**: Bootstrap 5.3.0
- **Icons**: Font Awesome 6.4.0
- **Styling**: Custom CSS with Bootstrap components
- **State Management**: Blazor component state and dependency injection

### Project Structure
```
IELTSReadingApp/
├── Components/
│   ├── Layout/
│   │   ├── MainLayout.razor      # Main application layout
│   │   └── NavMenu.razor         # Navigation sidebar
│   └── Pages/
│       ├── Home.razor            # Dashboard with test cards
│       └── Test.razor            # Test interface
├── Models/
│   └── IELTSModels.cs           # Data models for tests and questions
├── Services/
│   └── TestDataService.cs       # Test data provider service
├── wwwroot/                     # Static files
├── Program.cs                   # Application startup
└── README.md                    # This file
```

### Key Components

#### Models (`IELTSModels.cs`)
- `ReadingTest` - Complete test with metadata
- `ReadingSection` - Individual reading passage with questions
- `ReadingPassage` - Text content with vocabulary
- `Question` - Individual question with type and answers
- `TestResult` - Scoring and timing results
- `BandScoreCalculator` - Official IELTS band score conversion

#### Services (`TestDataService.cs`)
- Provides 10 comprehensive practice tests
- Includes authentic academic content
- Covers diverse topics (science, history, technology, etc.)
- Implements all 11 IELTS question types

#### Pages
- `Home.razor` - Responsive card-based dashboard
- `Test.razor` - Full-featured test interface with timer

## 📊 Scoring System

The application uses the official IELTS Academic Reading band score conversion:
- **39-40 correct**: Band 9.0
- **37-38 correct**: Band 8.5
- **36 correct**: Band 8.0
- **34-35 correct**: Band 7.5
- **32-33 correct**: Band 7.0
- **30-31 correct**: Band 6.5
- **27-29 correct**: Band 6.0
- **23-26 correct**: Band 5.5
- **19-22 correct**: Band 5.0
- **15-18 correct**: Band 4.0
- **13-14 correct**: Band 3.5
- **10-12 correct**: Band 3.0
- **8-9 correct**: Band 2.5
- **6-7 correct**: Band 2.0
- **4-5 correct**: Band 1.5
- **0-3 correct**: Band 1.0

## 🎯 Educational Value

### Skills Developed
- **Reading comprehension** at academic level
- **Time management** under exam conditions
- **Question type familiarity** with all IELTS formats
- **Academic vocabulary** through diverse passages
- **Test-taking strategies** through repeated practice

### Content Areas Covered
- Environmental Science & Sustainability
- History & Archaeology
- Technology & Innovation
- Biology & Marine Science
- Urban Planning & Architecture
- Psychology & Neuroscience
- Space Science & Astronomy
- Medicine & Public Health
- Literature & Cultural Studies
- Economics & Business

## 🔧 Customization

### Adding New Tests
1. Create test data in `TestDataService.cs`
2. Follow the existing pattern for `ReadingTest` objects
3. Include authentic academic passages (800-950 words each)
4. Add diverse question types for comprehensive practice

### Modifying UI
- Edit Bootstrap classes in Razor components
- Customize colors and styling in component `<style>` sections
- Add new Font Awesome icons as needed
- Modify responsive breakpoints for different devices

### Extending Functionality
- Add user authentication for progress tracking
- Implement answer review with explanations
- Add detailed performance analytics
- Create practice mode vs. test mode options

## 📱 Browser Compatibility

- **Chrome 90+** ✅ Fully supported
- **Firefox 88+** ✅ Fully supported
- **Safari 14+** ✅ Fully supported
- **Edge 90+** ✅ Fully supported
- **Mobile browsers** ✅ Responsive design

## 🤝 Contributing

Contributions are welcome! Areas for improvement:
- Additional practice tests with diverse content
- Enhanced answer explanations
- Performance analytics dashboard
- User progress tracking
- Mobile app version
- Offline capability

## 📄 License

This project is provided for educational purposes. IELTS is a trademark of the British Council, IDP Education, and Cambridge Assessment English.

## 🙏 Acknowledgments

- Test content inspired by authentic IELTS Academic Reading materials
- UI design follows modern web application best practices
- Scoring system based on official IELTS band descriptors
- Question types match official IELTS specifications

## 📞 Support

For technical issues or questions about the application:
1. Check the browser console for error messages
2. Ensure .NET 8.0 SDK is properly installed
3. Verify all dependencies are restored with `dotnet restore`
4. Clear browser cache if experiencing display issues

---

**Happy studying and good luck with your IELTS Academic Reading preparation!** 🎓📚