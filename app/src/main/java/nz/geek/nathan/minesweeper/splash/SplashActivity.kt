package nz.geek.nathan.minesweeper.splash

import android.os.Bundle
import nz.geek.nathan.minesweeper.app.MSApplication
import nz.geek.nathan.minesweeper.base.BaseActivity
import nz.geek.nathan.minesweeper.base.BaseContract
import org.jetbrains.anko.linearLayout
import org.jetbrains.anko.matchParent
import javax.inject.Inject

/**
 * Created by nate on 17/05/17.
 */
class SplashActivity: BaseActivity<SplashContract.Presenter>(), SplashContract.View {

    override fun setupActivityComponent() {
        MSApplication.get(this)
                .mApplicationComponent
                .plus(SplashModule(this))
                .inject(this)
    }

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(linearLayout {
            lparams { height = matchParent; width = matchParent;  }
        })

    }
}