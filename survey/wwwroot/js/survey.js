// Track which Facebook group they came from (if available from URL params)
window.addEventListener('DOMContentLoaded', () => {
    const urlParams = new URLSearchParams(window.location.search);
    const source = urlParams.get('source') || urlParams.get('ref') || 'direct';
    document.getElementById('referralSource').value = source;
});

// Show/hide conditional questions based on AI usage
document.getElementById('currentlyUsesAI').addEventListener('change', function() {
    const aiUserQuestions = document.getElementById('aiUserQuestions');
    const nonAiUserQuestions = document.getElementById('nonAiUserQuestions');

    if (this.checked) {
        aiUserQuestions.style.display = 'block';
        nonAiUserQuestions.style.display = 'none';
    } else {
        aiUserQuestions.style.display = 'none';
        nonAiUserQuestions.style.display = 'block';
    }
});

// Show/hide email field based on alpha testing interest
document.getElementById('interestedInAlphaTesting').addEventListener('change', function() {
    const alphaDetails = document.getElementById('alphaDetails');
    const emailField = document.getElementById('email');

    if (this.checked) {
        alphaDetails.style.display = 'block';
        emailField.required = true;
    } else {
        alphaDetails.style.display = 'none';
        emailField.required = false;
    }
});

// Handle form submission
document.getElementById('surveyForm').addEventListener('submit', async function(e) {
    e.preventDefault();

    const submitBtn = this.querySelector('.submit-btn');
    submitBtn.disabled = true;
    submitBtn.textContent = 'Submitting...';

    try {
        // Collect form data
        const formData = {
            ageRange: document.getElementById('ageRange').value || null,
            generalLocation: document.getElementById('generalLocation').value || null,
            occupationCategory: document.getElementById('occupationCategory').value || null,
            techComfortLevel: document.getElementById('techComfortLevel').value || null,

            currentlyUsesAI: document.getElementById('currentlyUsesAI').checked,
            aiToolsUsed: document.getElementById('aiToolsUsed').value || null,
            primaryAITool: document.getElementById('primaryAITool').value || null,
            frequencyOfUse: document.getElementById('frequencyOfUse').value || null,

            useForWork: document.querySelector('input[name="useForWork"]').checked,
            useForPersonalProjects: document.querySelector('input[name="useForPersonalProjects"]').checked,
            useForLearning: document.querySelector('input[name="useForLearning"]').checked,
            useForCreativeWork: document.querySelector('input[name="useForCreativeWork"]').checked,
            useForCoding: document.querySelector('input[name="useForCoding"]').checked,
            useForWriting: document.querySelector('input[name="useForWriting"]').checked,
            useForResearch: document.querySelector('input[name="useForResearch"]').checked,
            useForOther: document.querySelector('input[name="useForOther"]').checked,
            otherUseCase: document.querySelector('input[name="otherUseCase"]').value || null,

            barrierCost: document.querySelector('input[name="barrierCost"]').checked,
            barrierComplexity: document.querySelector('input[name="barrierComplexity"]').checked,
            barrierPrivacyConcerns: document.querySelector('input[name="barrierPrivacyConcerns"]').checked,
            barrierDontKnowHowToStart: document.querySelector('input[name="barrierDontKnowHowToStart"]').checked,
            barrierDontSeeBenefit: document.querySelector('input[name="barrierDontSeeBenefit"]').checked,
            barrierOther: document.querySelector('input[name="barrierOther"]').checked,
            otherBarrier: document.querySelector('input[name="otherBarrier"]').value || null,

            biggestFrustration: document.getElementById('biggestFrustration').value || null,
            whatsMissing: document.getElementById('whatsMissing').value || null,
            idealAIFeatures: document.getElementById('idealAIFeatures').value || null,

            interestedInAlphaTesting: document.getElementById('interestedInAlphaTesting').checked,
            email: document.getElementById('email').value || null,
            additionalComments: document.getElementById('additionalComments').value || null,

            referralSource: document.getElementById('referralSource').value
        };

        // Validate email if interested in alpha testing
        if (formData.interestedInAlphaTesting && !formData.email) {
            alert('Please provide your email address to be considered for alpha testing.');
            submitBtn.disabled = false;
            submitBtn.textContent = 'Submit Survey';
            return;
        }

        // Submit to API
        const response = await fetch('/api/survey/submit', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(formData)
        });

        if (!response.ok) {
            const error = await response.json();
            throw new Error(error.error || 'Failed to submit survey');
        }

        // Show success message
        document.getElementById('surveyForm').style.display = 'none';
        document.getElementById('successMessage').style.display = 'block';

        // Scroll to top
        window.scrollTo({ top: 0, behavior: 'smooth' });

    } catch (error) {
        console.error('Error submitting survey:', error);
        alert('There was an error submitting your survey. Please try again.');
        submitBtn.disabled = false;
        submitBtn.textContent = 'Submit Survey';
    }
});
